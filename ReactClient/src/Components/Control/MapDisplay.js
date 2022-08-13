import React, { useEffect, useState, useRef, useMemo } from 'react';
import { useSimConnectData, simConnectGetFlightPlan } from '../../Services/SimConnectDataProvider';
import { useLocalStorageData } from '../../Services/LocalStorageProvider';
import { useInterval } from '../Util/hooks';
import { LayersControl, LayerGroup, TileLayer, useMapEvents } from 'react-leaflet'
import L from 'leaflet';
import 'leaflet.marker.slideto';
import 'leaflet-marker-rotation';
import 'leaflet-easybutton';
import '@elfalem/leaflet-curve';
import { getDistance } from 'geolib';
import {BingLayer} from 'react-leaflet-bing-v2';

const MAP_TYPE_DEFAULTS = { zoomLevel: 12, flightFollowing: true, showFlightPlan: true, uiZoomFactor: 1, planeRadiusCircleRange: 2.5};

const MAP_PROVIDER = {
    openTopo: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
    openStreet: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
    dark: 'https://tiles.stadiamaps.com/tiles/alidade_smooth_dark/{z}/{x}/{y}{r}.png',
    googleStreet: 'http://mt0.google.com/vt/lyrs=m&hl=en&x={x}&y={y}&z={z}',
    googleTerrain: 'http://mt0.google.com/vt/lyrs=p&hl=en&x={x}&y={y}&z={z}',
    googleSatellite: 'http://mt0.google.com/vt/lyrs=s&hl=en&x={x}&y={y}&z={z}',
    googleHybrid: 'http://mt0.google.com/vt/lyrs=y&hl=en&x={x}&y={y}&z={z}',
    aviation: 'http://{s}.tile.maps.openaip.net/geowebcache/service/tms/1.0.0/openaip_basemap@EPSG%3A900913@png/{z}/{x}/{y}.png',
}

const getPlaneRadiusCircleTooltip = (text) =>
{
    let tooltip = `<div style="margin:0; padding: 2px; background-color:black; color:#33CEFF; font-size:1em; font-weight: bold">${text}</div>`;
    return tooltip;
}

const drawPlaneCircleRadius = (map, planePosition, scaleInNm = 2.5, isCreated, marker) => {
    // About radius of 178 pixels for a zoom level of 12 has the desire 2.5nm circle radius on screen size
    // Zoom level of 12 is equal to 2.5nm
    // ** Since I can't control exact zoom level in small fractionin leaflet, the radius numbers won't be nicely rounded
    
    let scale = 2.5 / scaleInNm;
    let circlePixelRadius = 178 * scale;
    let rangeRadius = 2.5 * Math.pow(2, 12 - map.getZoom()) * scale ;
    let formattedRangeRadius;
    let rangeUnit;

    if(rangeRadius >= 5)
    {
        rangeUnit = 'NM';
        formattedRangeRadius = rangeRadius.toFixed(0);
    }
    else if(rangeRadius >= 2.5)
    {
        rangeUnit = 'NM';   
        formattedRangeRadius = rangeRadius.toFixed(1);
    }
    else if(rangeRadius >= 1.25)
    {
        rangeUnit = 'NM';
        formattedRangeRadius = rangeRadius.toFixed(2);
    }
    else if(rangeRadius <= 0.625)
    {
        rangeUnit = 'FT';
        formattedRangeRadius = (rangeRadius * 6076.12).toFixed(0)
    }

    // draw a range circle
    if(isCreated)
    {
        return L.circleMarker(planePosition, {radius: circlePixelRadius, color: 'white', fill: false}).bindTooltip(toolTip, { permanent: true, direction: 'right',  offset: [-155 * scale, -130 * scale] });
    }

    let toolTip = getPlaneRadiusCircleTooltip(formattedRangeRadius + rangeUnit);
    marker.radius = circlePixelRadius;
    marker.unbindTooltip();
    marker.bindTooltip(toolTip, { permanent: true, direction: 'right',  offset: [-155 * scale, -130 * scale] });
    return marker;
}

const getControlPoints = (waypoints) => {
    // This is to create two control points XXXX meters away any give waypoint to give flight path
    // a smooth quadratic bezier curve transition. You can adjust the bezier curve radius with the constant below
    const bezierCurveRadius = 100;

    let controlPoints = [];
        for (var i = 0; i < waypoints.length; i++) {
            let prevWp = i === 0 ? null : waypoints[i - 1].latLong;
            let curWp = waypoints[i].latLong;
            let nextWp = i === waypoints.length - 1 ? null : waypoints[i + 1].latLong;

            var distance1 = prevWp === null ? null : getDistance({ latitude: prevWp[0], longitude: prevWp[1] }, { latitude: curWp[0], longitude: curWp[1] });
            var distance2 = nextWp === null ? null : getDistance({ latitude: curWp[0], longitude: curWp[1] }, { latitude: nextWp[0], longitude: nextWp[1] });

            let ratio1 = (distance1 / bezierCurveRadius) > 2 ? (distance1 / bezierCurveRadius) : 2;
            let ratio2 = (distance2 / bezierCurveRadius) > 2 ? (distance2 / bezierCurveRadius) : 2;

            var p1 = prevWp === null ? null : [(prevWp[0] - curWp[0]) / ratio1 + curWp[0], (prevWp[1] - curWp[1]) / ratio1 + curWp[1]];
            var p2 = nextWp === null ? null : [(nextWp[0] - curWp[0]) / ratio2 + curWp[0], (nextWp[1] - curWp[1]) / ratio2 + curWp[1]];

            controlPoints.push({ p1: p1, p2: p2 });
        }

    return controlPoints;
}

const drawFlightPath = (waypoints, layerGroup, scaleInNm) => {
    let scale = 2.5 / scaleInNm;
    let path, line, marker, tooltip;
    let controlPoints = getControlPoints(waypoints);

    waypoints.forEach((waypoint, index) => {
        let controlPoint = controlPoints[index];
        let nextControlPoint = controlPoints[index + 1];

        let lineColor = Boolean(waypoint.isActiveLeg) ? 'magenta' : 'magenta'

        // First waypoint
        if (controlPoint.p1 === null) {
            path = [waypoint.latLong, controlPoint.p2];
            line = new L.Polyline(path, {color: lineColor});
            layerGroup.addLayer(line);
        }
        // Last waypoint
        else if (controlPoint.p2 === null) {
            path = [controlPoint.p1, waypoint.latLong];
            line = new L.Polyline(path, {color: lineColor});
            layerGroup.addLayer(line);
        }
        // All other waypoints inbetween, draw bezier curve
        else {
            path = ['M', controlPoint.p1, 'Q', waypoint.latLong, controlPoint.p2];
            line = L.curve(path, {color: 'white'});
            layerGroup.addLayer(line);
        }

        // Waypoint marker
        tooltip = getFullmapTooltip(waypoint);
        marker = L.circleMarker(waypoint.latLong, {radius: 6, color: 'purple'}).bindTooltip(tooltip, { permanent: true, interactive: true, offset: [-50 * scale, -15 * scale] });
        
        layerGroup.addLayer(marker);

        // Draw inbetween control points line
        if (index < waypoints.length - 1) {
            path = [controlPoint.p2, nextControlPoint.p1];
            line = new L.Polyline(path, {color: lineColor});
            layerGroup.addLayer(line);
        }
    })
}

const getFullmapTooltip = (waypoint) => {
    let tooltip = `<div style="padding: 3px; font-size:1em; font-weight: bold">${waypoint.id}</div>`;
    return tooltip;
}

const centerPlaneToMap = (map, mapPosition, planePosition) => {
    mapPosition.current = planePosition.current;
    if (mapPosition.current !== null)
        map.panTo(mapPosition.current);
}

const PLANE_ICON_DARK = L.icon({
    iconUrl: '/img/airplane-dark.png',
    iconSize: [36, 36],
    iconAnchor: [18, 18]
});

const PLANE_ICON_LIGHT = L.icon({
    iconUrl: '/img/airplane-light.png',
    iconSize: [36, 36],
    iconAnchor: [18, 18]
});

const formatLatLong = (lat, lon) => {
    if(lat === undefined || lon === undefined)
        return [0,0];

    return [lat, lon]
    //return [lat * 180 / Math.PI, lon * 180 / Math.PI]
}

let centerPlaneIcon, flightFollowingIcon, showFlightPlanIcon;

const MapDisplay = ({refresh}) => {
    const { simConnectData, simConnectSystemEvent } = useSimConnectData();
    const { PLANE_HEADING_TRUE, GPS_LAT, GPS_LON } = simConnectData;
    const { mapConfig, configurationData } = useLocalStorageData();
    const { mapRefreshInterval } = configurationData;
    
    const [ mapDefaults ] = useState(MAP_TYPE_DEFAULTS);
    const [ flightFollowing, setFlightFollowing ] = useState(MAP_TYPE_DEFAULTS.flightFollowing);  
    const [ showFlightPlan, setShowFlightPlan ] = useState(MAP_TYPE_DEFAULTS.showFlightPlan);
    const [ waypoints, setWaypoints] = useState([]);

    const planePosition = useRef(formatLatLong(GPS_LAT, GPS_LON));
    const mapPosition = useRef(formatLatLong(GPS_LAT, GPS_LON));

    //const map = useMap();
    const layerGroupFlightPlan = useRef();
    const layerGroupPlanePosition = useRef();
    const layerGroupPlaneMarker = useRef();
    const layerGroupPlaneMarkerLegend = useRef();
    const layerGroupPlaneCircle = useRef();
    const layerGroupPlaneKeepAtCenter = useRef(false);
    const waypointLegend = useRef();
    const activeMapLayer = useRef('Bing Roads');
    const bingKey = window._BINGKEY;

    const map = useMapEvents({
        dragstart: (e) => {
            setFlightFollowing(false);
        },
        click: (e) => {
            layerGroupPlaneKeepAtCenter.current = false;
        }
    });

    useEffect(() => {
        // configure map container
        map.attributionControl.setPrefix(); // remove leaflet logo
       
        layerGroupFlightPlan.current = L.layerGroup();
        map.addLayer(layerGroupFlightPlan.current);
    
        layerGroupPlanePosition.current = L.layerGroup();
        map.addLayer(layerGroupPlanePosition.current);

        layerGroupPlaneMarker.current = L.rotatedMarker([0, 0], { icon: PLANE_ICON_DARK, rotationAngle: 0, rotationOrigin: 'center' });
        map.addLayer(layerGroupPlaneMarker.current);

        layerGroupPlaneCircle.current = drawPlaneCircleRadius(map, [0,0], mapDefaults.planeRadiusCircleRange, true, null);
        layerGroupPlanePosition.current.addLayer(layerGroupPlaneCircle.current);

        layerGroupPlaneKeepAtCenter.current = true;

        layerGroupPlaneMarkerLegend.current = L.layerGroup();
        map.addLayer(layerGroupPlaneMarkerLegend.current);

        waypointLegend.current = L.control({ position: 'topright' });
        waypointLegend.current.onAdd = () => {
            var div = L.DomUtil.create('div', 'waypointLegend');
            div.setAttribute('id', 'waypointLegend');
            div.style.display = 'none';
            return div;
        };
        waypointLegend.current.addTo(map);

        map.setZoom(mapDefaults.zoomLevel);

        map.setView(planePosition.current);

        document.getElementsByClassName('leaflet-container')[0].style.zoom = mapDefaults.uiZoomFactor;
    }, [])

    useEffect(() => {
        map.invalidateSize();
    }, [refresh])

    useEffect(() => {
        map.on("baselayerchange", e => {
            switch (e.name) {
                case 'Dark':
                case 'Google Satellite':
                case 'Google Hybrid':
                case 'Bing Aerial':
                case 'Bing Aerial with Labels':
                    layerGroupPlaneMarker.current.setIcon(PLANE_ICON_LIGHT);
                    break;
                default:
                    layerGroupPlaneMarker.current.setIcon(PLANE_ICON_DARK);
                    break;
            }

            activeMapLayer.current = mapConfig.currentLayer;
        });
    }, [map])

    useEffect(() => {
        if(centerPlaneIcon !== undefined) map.removeControl(centerPlaneIcon);
        if(flightFollowingIcon !== undefined) map.removeControl(flightFollowingIcon);
        if(showFlightPlanIcon !== undefined) map.removeControl(showFlightPlanIcon);

        centerPlaneIcon = L.easyButton('<span class="material-icons leaflet-icon">center_focus_weak</span>', () => {
            centerPlaneToMap(map, mapPosition, planePosition);
        }).addTo(map);

        flightFollowingIcon = L.easyButton(`<span class="material-icons leaflet-icon">${flightFollowing ? 'airplanemode_active' : 'airplanemode_inactive'}</span>`, () => {
            setFlightFollowing(!flightFollowing);
            layerGroupPlaneKeepAtCenter.current = !flightFollowing;
        }).addTo(map);

        showFlightPlanIcon = L.easyButton(`<span class="material-icons leaflet-icon">${showFlightPlan ? 'content_paste' : 'content_paste_off'}</span>`, () => {
            setShowFlightPlan(!showFlightPlan);
        }).addTo(map);

    }, [map, flightFollowing, showFlightPlan])


    useInterval(() => {
        let newPosition = formatLatLong(GPS_LAT, GPS_LON);
        planePosition.current = newPosition;
    }, mapRefreshInterval);

    useEffect(() => {
        if (flightFollowing)
        {
            map.panTo(planePosition.current, {animate: false});

            layerGroupPlaneCircle.current = drawPlaneCircleRadius(map, planePosition.current, mapDefaults.planeRadiusCircleRange, false, layerGroupPlaneCircle.current);
            layerGroupPlaneCircle.current.slideTo(planePosition.current, { duration: mapRefreshInterval, keepAtCenter: layerGroupPlaneKeepAtCenter.current });

            if (planePosition.current !== null) {
                layerGroupPlaneMarker.current.setRotationAngle(PLANE_HEADING_TRUE);
                layerGroupPlaneMarker.current.slideTo(planePosition.current, { duration: mapRefreshInterval, keepAtCenter: layerGroupPlaneKeepAtCenter.current });
            }
        }
    }, [flightFollowing, planePosition.current])

    useEffect(async() => {
        let data = await simConnectGetFlightPlan();
          
        if (data !== undefined && data !== null) {
            setWaypoints(data.waypoints);
        }
    }, [simConnectSystemEvent, refresh])

    useEffect(() => {
        if(waypoints != null && waypoints.length > 1)
        {
            layerGroupFlightPlan.current.clearLayers();
       
            if(showFlightPlan)
                drawFlightPath(waypoints, layerGroupFlightPlan.current, mapDefaults.planeRadiusCircleRange)
            else
                layerGroupFlightPlan.current.clearLayers();
        
        }
    }, [showFlightPlan, waypoints])

    return useMemo(() => (
        <LayersControl>
            <LayersControl.BaseLayer name='Open Topo' checked={activeMapLayer.current === 'Open Topo'}>
                <TileLayer url={MAP_PROVIDER.openTopo} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Open Street' checked={activeMapLayer.current === 'Open Street'}>
                <TileLayer url={MAP_PROVIDER.openStreet} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Dark' checked={activeMapLayer.current === 'Dark'}>
                <TileLayer url={MAP_PROVIDER.dark} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Google Street' checked={activeMapLayer.current === 'Google Street'}>
                <TileLayer url={MAP_PROVIDER.googleStreet} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Google Terrain' checked={activeMapLayer.current === 'Google Terrain'}>
                <TileLayer url={MAP_PROVIDER.googleTerrain} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Google Satellite' checked={activeMapLayer.current === 'Google Satellite'}>
                <TileLayer url={MAP_PROVIDER.googleSatellite} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Google Hybrid' checked={activeMapLayer.current === 'Google Hybrid'}>
                <TileLayer url={MAP_PROVIDER.googleHybrid} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Bing Roads' checked={activeMapLayer.current === 'Bing Roads'}>
                <BingLayer  bingkey={bingKey} type="Road"/>
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Bing Aerial' checked={activeMapLayer.current === 'Bing Aerial'}>
                <BingLayer  bingkey={bingKey} />
            </LayersControl.BaseLayer>
            <LayersControl.BaseLayer name='Bing Aerial with Labels' checked={activeMapLayer.current === 'Bing Aerial with Labels'}>
                <BingLayer  bingkey={bingKey} type="AerialWithLabels" />
            </LayersControl.BaseLayer>
            <LayersControl.Overlay name='Aviation'>
                <TileLayer url={MAP_PROVIDER.aviation} tms={true} detectRetina={true} subdomains='12' />
            </LayersControl.Overlay>
            <LayersControl.Overlay checked={showFlightPlan} name='Flight Plan'>
                <LayerGroup id='lgFlightPlan' ref={layerGroupFlightPlan} />
            </LayersControl.Overlay>
        </LayersControl>
    ), [showFlightPlan, flightFollowing, layerGroupFlightPlan.current])
}

export default MapDisplay;