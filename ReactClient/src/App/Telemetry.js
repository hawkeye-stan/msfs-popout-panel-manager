import React, { useMemo } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { useSimConnectData } from '../Services/SimConnectDataProvider';
import { Typography } from '@mui/material';
import Grid from '@mui/material/Grid';
import StopWatch from '../Components/Control/StopWatch';

const useStyles = makeStyles((theme) => ({
    root: {
        display: 'flex',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between'
    },
    sectionTelemetry: {
        display: 'flex',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
    },
    sectionSmall: {
        display: 'flex',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingLeft: '30px',
        paddingRight: '30px',
        minHeight: '1.5em',
        borderLeft: '1px solid gray',
        borderRight: '1px solid gray',
    },
    telemetryDisplay: {
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center'
    },
    segmentDisplay:
    {
        display: 'flex',
        justifyContent: 'center'
    },
    dataLabel:
    {
        color: 'rgb(32, 217, 32, 1)',
    }
}));

const Telemetry = () => {
    const classes = useStyles();

    const { ELEVATOR_TRIM, AILERON_TRIM, FLAPS, RUDDER_TRIM, BRAKE_PARKING_INDICATOR,
        GEAR_CENTER_POSITION, GEAR_LEFT_POSITION, GEAR_RIGHT_POSITION } = useSimConnectData().simConnectData;

    const gearColor = (gear) => {
        if (gear === 0)
            return 'rgb(97, 99, 105, 1)';
        else if (gear === 100)
            return 'rgb(32, 217, 32, 1)';

        return 'rgb(250, 186, 57, 1)';
    }

    const brakeColor = (brake) => {
        if (brake === 1)
            return 'rgb(255, 0, 0, 1)';

        return 'rgb(32, 217, 32, 1)';
    }

    return useMemo(() => (
        <Grid container className={classes.root}>
            <Grid item xs={5} className={classes.sectionTelemetry}>
                <Grid item xs={4} className={classes.sectionSmall}>
                    <Typography variant='body1'>Elevator Trim:</Typography>
                    <Typography variant='body1' className={classes.dataLabel}>{ELEVATOR_TRIM === undefined ? 0 : ELEVATOR_TRIM}</Typography>
                </Grid>
                <Grid item xs={4} className={classes.sectionSmall}>
                    <Typography variant='body1'>Aileron Trim:</Typography>
                    <Typography variant='body1' className={classes.dataLabel}>{AILERON_TRIM === undefined ? 0 : Math.abs(AILERON_TRIM)} {AILERON_TRIM > 0 ? 'R' : AILERON_TRIM < 0 ? 'L' : ''}</Typography>
                </Grid>
                <Grid item xs={4} className={classes.sectionSmall}>
                    <Typography variant='body1'>Rudder Trim:</Typography>
                    <Typography variant='body1' className={classes.dataLabel}>{RUDDER_TRIM === undefined ? 0 : Math.abs(RUDDER_TRIM)} {RUDDER_TRIM > 0 ? 'R' : RUDDER_TRIM < 0 ? 'L' : ''}</Typography>
                </Grid>
            </Grid>
            <Grid item xs={7} className={classes.sectionTelemetry}>
                <Grid item xs={2} className={classes.sectionSmall}>
                    <Typography variant='body1'>Flap:</Typography>
                    <Typography variant='body1' className={classes.dataLabel}>{FLAPS === undefined ? 0 : FLAPS}</Typography>
                </Grid>
                <Grid item xs={2} className={classes.sectionSmall}>
                    <Typography variant='body1'>Brake:</Typography>
                    <Typography variant='body1' className={classes.dataLabel} style={{ color: brakeColor(BRAKE_PARKING_INDICATOR) }}>{BRAKE_PARKING_INDICATOR === undefined ? 'N/A' : (BRAKE_PARKING_INDICATOR === 0 ? 'OFF' : 'ON')}</Typography>
                </Grid>
                <Grid item xs={3} className={classes.sectionSmall}>
                    <Typography variant='body1'>Gear:</Typography>
                    <div><Typography variant='body1' className={classes.dataLabel} style={{ color: gearColor(GEAR_LEFT_POSITION) }}>L</Typography></div>
                    <div><Typography variant='body1' className={classes.dataLabel} style={{ color: gearColor(GEAR_CENTER_POSITION) }}>N</Typography></div>
                    <div><Typography variant='body1' className={classes.dataLabel} style={{ color: gearColor(GEAR_RIGHT_POSITION) }}>R</Typography></div>
                </Grid>
                <Grid item xs={5} className={classes.sectionSmall}>
                    <Typography variant='body1'>Timer:</Typography>
                    <StopWatch></StopWatch>
                </Grid>
            </Grid>
        </Grid>
    ), [classes, FLAPS, ELEVATOR_TRIM, AILERON_TRIM, RUDDER_TRIM, BRAKE_PARKING_INDICATOR,
        GEAR_CENTER_POSITION, GEAR_LEFT_POSITION, GEAR_RIGHT_POSITION])
}

export default Telemetry
