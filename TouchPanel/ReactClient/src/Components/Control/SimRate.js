import React, { useMemo } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { useSimConnectData } from '../../Services/SimConnectDataProvider';
import { simActions } from '../../Services/ActionProviders/simConnectActionHandler';
import { Typography } from '@mui/material';
import NumericEntryDisplay from '../Control/NumericEntryDisplay';

const useStyles = makeStyles((theme) => ({
    section: theme.custom.panelSection,
    simrateEntry: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'space-around',
        alignItems: 'center'
    }
}));

const postSimRateChangeAction = (currentSimRate, newSimRate) => {
    let step = Math.log2(newSimRate) - Math.log2(currentSimRate);

    for (let i = 0; i < Math.abs(step); i++) {
        Math.sign(step) === 1 ? simActions.SimRate.increase() : simActions.SimRate.decrease();
    }
}

const SimRate = () => {
    const classes = useStyles();
    const { SIMULATION_RATE } = useSimConnectData().simConnectData;

    const handleOnSet = (newSimRate, currentSimRate) => {
        postSimRateChangeAction(currentSimRate, newSimRate);
    }

    return useMemo(() => (
        <div className={classes.section}>
            <Typography variant='body1'>SIM RATE</Typography>
            <div className={classes.simrateEntry}>
                <NumericEntryDisplay
                    initialValue={SIMULATION_RATE}
                    numberOfDigit={1}
                    decimalPlaces={1}
                    minValue={1}
                    maxValue={8}
                    usedByArduino={false}
                    disableNumPadKeys={[9, 0, '-']}
                    onColor={'rgb(32, 217, 32, 1)'}
                    onSet={(value) => handleOnSet(value, SIMULATION_RATE)} />
                <div style={{ paddingLeft: '0.25em' }}>
                    <Typography variant='body1'>X</Typography>
                </div>
            </div>

        </div>
    ), [classes, SIMULATION_RATE])
}

export default SimRate;