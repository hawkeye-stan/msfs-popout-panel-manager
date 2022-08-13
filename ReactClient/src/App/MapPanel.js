import React, {useState, useEffect, useMemo} from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { MapContainer } from 'react-leaflet';
import { useSimConnectData } from '../Services/SimConnectDataProvider';
import MapDisplay from '../Components/Control/MapDisplay';

const useStyles = makeStyles((theme) => ({
    root: {
        display: 'flex',
        flexDirection: 'column',
        flexGrow: 1,
        height: '100%',
        border: '1px solid ' + theme.palette.divider,
        borderRadius: theme.spacing(1),
        touchAction: 'none'
    },
}));

const MapPanel = ({refresh}) => {
    const { simConnectSystemEvent } = useSimConnectData();
    const classes = useStyles();
    const [reload, setReload] = useState(true);

    useEffect(() =>{
        if(simConnectSystemEvent !== null)
        {
            if(simConnectSystemEvent === 'SIMSTART')
                setReload(true);
            else if(simConnectSystemEvent === 'SIMSTOP')
                setReload(false);
        }
    }, [simConnectSystemEvent])

    return useMemo(() => (
        <div className={classes.root}>
            { reload && 
                <MapContainer zoom={15} scrollWheelZoom={true} style={{ height: '100%'}}>
                    <MapDisplay refresh={refresh} />
                </MapContainer>
            }
        </div>
    ), [refresh])
}

export default MapPanel;

