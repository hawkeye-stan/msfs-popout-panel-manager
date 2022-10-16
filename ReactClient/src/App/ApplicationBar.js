import React, {  useMemo } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { useSimConnectData } from '../Services/SimConnectDataProvider';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Grid from '@mui/material/Grid';
import IconButton from '@mui/material/IconButton';
import UsbIcon from '@mui/icons-material/Usb';
import MapIcon from '@mui/icons-material/Map';
import NetworkCheckIcon from '@mui/icons-material/NetworkCheck';
import { Typography } from '@mui/material';
import Telemetry from './Telemetry';

const useStyles = makeStyles(() => ({
    toolbar: {
        minHeight: '1.5em',
        padding: 0
    },
    menuIcons: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'flex-end',
        alignItems: 'center',
    },
    statusIcons: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'flex-start',
        alignItems: 'center',
    },
    menuButton: {
        marginRight: '0.25em',
        marginLeft: '0.25em'
    },
    networkConnected: {
        color: 'lightgreen'
    },
    networkDisconnected: {
        color: 'red'
    },
    drawer: {
        width: 250,
    },
    simRate:{
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
    },
    simRateData:
    {
        marginLeft: '0.5em',
        color: 'rgb(32, 217, 32, 1)'
    }
}));

const ApplicationBar = ({mapOpenChanged, panelProfile}) => {
    const classes = useStyles();
    const { networkStatus, arduinoStatus, simConnectData } = useSimConnectData();
    const { SIM_RATE } = simConnectData;

    const displaySimRate = (simRate) => {
        if(simRate === undefined)
            return 1;

        var value = Number(simRate);

        if(value >= 1)
            return value.toFixed(0);
        else if (value >= 0.5)
            return value.toFixed(1);
        
        return value.toFixed(2);
    }

    return useMemo(() => (
        <div className={classes.root}>
            <AppBar position='static'>
                <Toolbar className={classes.toolbar}>
                    <Grid container>
                        <Grid item xs={1} className={classes.statusIcons}>
                            <IconButton color='inherit' aria-label='network status' size='small' className={classes.menuButton}>
                                <NetworkCheckIcon className={networkStatus ? classes.networkConnected : classes.networkDisconnected} />
                            </IconButton>
                            <IconButton color='inherit' aria-label='arduino status' size='small' className={classes.menuButton}>
                                <UsbIcon className={arduinoStatus ? classes.networkConnected : classes.networkDisconnected} />
                            </IconButton>
                            {panelProfile.enableMap && 
                                <IconButton color='inherit' aria-label='map' size='small' className={classes.menuButton} onClick={() => mapOpenChanged()}>
                                    <MapIcon />
                                </IconButton>
                            }
                        </Grid>
                        <Grid item xs={10}>
                            <Telemetry simConnectData={simConnectData}></Telemetry>
                        </Grid>
                        <Grid item xs={1} className={classes.menuIcons}>
                            <Grid item xs={12} className={classes.simRate}>
                                <Typography variant='body1'>Sim Rate:</Typography>
                                <Typography variant='body1' className={classes.simRateData}>x{displaySimRate(SIM_RATE)}</Typography>
                            </Grid>
                        </Grid>
                    </Grid>
                </Toolbar>
            </AppBar>
        </div >
    ), [classes, networkStatus, arduinoStatus, panelProfile, mapOpenChanged, SIM_RATE]);
}

export default ApplicationBar;