import React from 'react';
import makeStyles from '@mui/styles/makeStyles';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Grid';
import NumericEntryDisplay from './NumericEntryDisplay';
import { useLocalStorageData } from '../../Services/LocalStorageProvider';

const useStyles = makeStyles((theme) => ({
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

const NumericItem = (props) => {
    const classes = useStyles();
    const { configurationData, updateConfigurationData, planeProfile } = useLocalStorageData();

    const handleChange = (key, value = null) => {
        let updatedData = { ...configurationData };

        switch (key) {
            case 'dataRefreshInterval':
            case 'mapRefreshInterval':
                updatedData[key] = value;
                break;
            case 'isUsedArduino':
            case 'showLog':
                updatedData[key] = !configurationData[key];
                break;
            default:
                updatedData.panelVisibility[planeProfile][key] = !configurationData.panelVisibility[planeProfile][key]
                break;
        }

        updateConfigurationData(updatedData);
    }

    return (
        <Grid container>
            <Grid item xs={6} className={classes.gridItemKey}>
                <Typography variant='body1'>{props.itemLabel}</Typography>
            </Grid>
            <Grid item xs={6} className={classes.gridItemValueLabel}>
                <div style={{ margin: '2px', padding: '4px' }}>
                    <NumericEntryDisplay
                        initialValue={configurationData[props.itemKey]}
                        numberOfDigit={4}
                        numberOfDisplayDigit={4}
                        decimalPlaces={0}
                        minValue={props.minInterval}
                        maxValue={props.maxInterval}
                        usedByArduino={false}
                        onSet={(value) => handleChange(props.itemKey, (Number(value)))}
                    />
                </div>
                <Typography variant='body1'>ms</Typography>
            </Grid>
        </Grid>
    )
}

export default NumericItem;