import React, { useState, useEffect } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import Container from '@mui/material/Container';
import CssBaseline from '@mui/material/CssBaseline';
import LocalStorageProvider from '../Services/LocalStorageProvider';
import SimConnectDataProvider, { simConnectGetPlanePanelProfilesInfo } from '../Services/SimConnectDataProvider';
import { useWindowDimensions } from '../Components/Util/hooks';
import ApplicationBar from './ApplicationBar';
import MapPanel from './MapPanel';
import PopoutPanelContainer from './PopoutPanelContainer';

const useStyles = props => makeStyles((theme) => ({
    rootFullWidth: {
        [theme.breakpoints.up('sm')]: { fontSize: '12px' },
        [theme.breakpoints.up('md')]: { fontSize: '16px' },
        [theme.breakpoints.up('lg')]: { fontSize: '18px' },
        [theme.breakpoints.up('xl')]: { fontSize: '18px' },
        padding: 0,
        maxWidth: '100vw',
        overflow: 'hidden',
        height: '100%'
    },
    appbar: {
        touchAction: 'none',
        position: 'fixed',
        width: '100vw',
        maxWidth: 'inherit'
    },
    mapPanel: {
        width: '100vw',
        height: '100%'
    },
    popoutPanel: {
        width: '100%',
        height: '100%'
    },
    panelContainer: {
        position: 'relative',
        backgroundColor: 'transparent',
        margin: '2em auto 0 auto',
        marginTop: '2em',
        height: `calc(${props.windowHeight - 1}px - 2em)`
    },
    panelContainerNoAppBar: {
        position: 'relative',
        backgroundColor: 'transparent',
        margin: '0em auto 0 auto',
        height: `calc(${props.windowHeight}px)`
    },
    framePanelContainer: {
        position: 'relative',
        backgroundColor: 'transparent',
        margin: '0 auto',
        height: `calc(${props.windowHeight - 1}px)`
    },
    subPanelBase: {
        position: 'absolute',
        backgroundRepeat: 'no-repeat',
        backgroundSize: '100%',
        width: '80%',
        height: '80%',
        
    },
}));

const WebPanel = ({ planeId, panelId }) => {
    const classes = useStyles(useWindowDimensions())();
    const [mapOpen, setMapOpen] = useState(false);
    const [panelProfile, setPanelProfile] = useState();

    document.body.style.backgroundColor = 'transparent';

    useEffect(() => {
        const getData = async () => {
            let data = await simConnectGetPlanePanelProfilesInfo();

            if (data !== null && data !== undefined) {
                // setup plane and panel profile
                let planeProfile = data.find(x => x.planeId.toLowerCase() === planeId);
                planeProfile.panels.forEach(x => x.planeId = planeId);      // adds plane id into panel object
    
                var panelProfile = planeProfile.panels.find(x => x.panelId.toLowerCase() === panelId);
    
                if (panelProfile !== undefined) {
                    setPanelProfile(panelProfile)
                }
            }
        }

        getData();

    }, [planeId, panelId]);

    const setupSubPanelClasses = () => {
        let styleClasses = [];
        styleClasses.push(classes.subPanelBase);
        return styleClasses;
    }

    const setupSubPanelLocationStyle = (subPanel) => {
        return { left: (subPanel.left / panelProfile.panelSize.width * 100.0) + '%', top: (subPanel.top / panelProfile.panelSize.height * 100.0) + '%' };
    }

    const setupSubPanelWidthHeightStyle = (subPanel) => {
        return { width: `calc(100% * ${subPanel.panelSize.width} / ${panelProfile.panelSize.width} * ${subPanel.scale})`, height: `calc(100% * ${subPanel.panelSize.height} / ${panelProfile.panelSize.height} * ${subPanel.scale})`};
    }

    const setupSubPanelDisplayStyle = () => {
        return { display: mapOpen ? 'none' : '' };
    }

    const setupMapDisplayStyle = () => {
        return { display: mapOpen ? '' : 'none' };
    }

    return (
        <LocalStorageProvider initialData={{}}>
            <SimConnectDataProvider>
                <CssBaseline />
                {panelProfile !== undefined &&
                    <Container className={classes.rootFullWidth}>
                        {panelProfile.showMenuBar &&
                            <div className={classes.appbar}>
                                <ApplicationBar mapOpenChanged={() => setMapOpen(!mapOpen)} panelProfile={panelProfile}></ApplicationBar>
                            </div>
                        }
                        <div className={panelProfile.showMenuBar ? classes.panelContainer : classes.panelContainerNoAppBar} style={{ aspectRatio: `${panelProfile?.panelSize?.width}/${panelProfile?.panelSize?.height}` }}>
                            <div className={classes.mapPanel} style={{ ...setupMapDisplayStyle() }}>
                                <MapPanel refresh={mapOpen} />
                            </div>
                            {panelProfile.subPanels.map(subPanel =>
                                <div key={subPanel.panelId} className={setupSubPanelClasses()} style={{...setupSubPanelDisplayStyle() , ...setupSubPanelLocationStyle(subPanel), ...setupSubPanelWidthHeightStyle(subPanel)} }>
                                    <PopoutPanelContainer panelInfo={subPanel} />
                                </div>
                            )}
                        </div>
                    </Container>
                }
                {
                    panelProfile === undefined &&
                    <div style={{ paddingLeft: '10px' }}>
                        <p>Loading panel....................</p>
                        <p>Panel:  {panelId}</p>
                    </div>
                }

            </SimConnectDataProvider>
        </LocalStorageProvider>
    )
}

export default WebPanel