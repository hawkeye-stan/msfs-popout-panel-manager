import React, { useState } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { useLocalStorageData } from '../Services/LocalStorageProvider';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Grid';
import SettingsIcon from '@mui/icons-material/Settings';
import SwipeableDrawer from '@mui/material/SwipeableDrawer';
import BooleanItem from '../Components/Control/BooleanItem';
import NumericItem from '../Components/Control/NumericItem';

const useStyles = makeStyles((theme) => ({
    root: {
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
    },
    panel: {
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'flex-start',
        alignItems: 'center',
        width: '300px'
    },
    grid: {
        padding: '8px'
    },
    gridTitle: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center',
        padding: '8px 0em',
        marginTop: '8px',
        borderTop: '1px solid ' + theme.palette.border,
        borderBottom: '1px solid ' + theme.palette.border
    },
    gridItemKey: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'flex-Start',
        alignItems: 'center',
        padding: '0 0 0 8px'
    },
    gridItemValueLabel: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center',
        padding: '0 8px 0 0'
    }
}));

const SettingConfiguration = () => {
    const classes = useStyles();
    const { configurationData } = useLocalStorageData();
    const [configSettingsIsOpen, setConfigSettingsIsOpen] = useState(false);

    return (
        <div className={classes.root}>
            <SettingsIcon onClick={() => setConfigSettingsIsOpen(true)} />
            <SwipeableDrawer
                anchor='right'
                open={configSettingsIsOpen}
                onClose={() => setConfigSettingsIsOpen(false)}
                onOpen={() => setConfigSettingsIsOpen(true)}>
                <div className={classes.panel}>
                    <Grid container className={classes.grid}>
                        <Grid item xs={12} className={classes.gridTitle}>
                            <Typography variant='h5'>Settings</Typography>
                        </Grid>
                        <NumericItem itemKey='dataRefreshInterval' itemLabel='Data Refresh Interval' minInterval={50} maxInterval={5000}></NumericItem>
                        <NumericItem itemKey='mapRefreshInterval' itemLabel='Map Refresh Interval' minInterval={50} maxInterval={5000}></NumericItem>
                        <BooleanItem itemKey='isUsedArduino' itemLabel='Use Arduino' onLabel='Yes' offLabel='No' onFunc={configurationData.isUsedArduino}></BooleanItem>
                        <BooleanItem itemKey='isEnabledSound' itemLabel='Enable Sound' onLabel='Yes' offLabel='No' onFunc={configurationData.isEnabledSound}></BooleanItem>
                    </Grid>
                </div>
            </SwipeableDrawer>
        </div>
    )
}

export default SettingConfiguration;
