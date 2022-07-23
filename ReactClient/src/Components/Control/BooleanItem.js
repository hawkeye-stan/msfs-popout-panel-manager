import React from 'react';
import makeStyles from '@mui/styles/makeStyles';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Grid';
import Switch from '@mui/material/Switch';
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

const BooleanItem = (props) => {
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
            case 'isEnabledSound':
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
            <Grid container item xs={6} alignItems="center">
                <Grid item xs={4} className={classes.gridItemValueLabel}>
                    <Typography variant='body1'>{props.offLabel}</Typography>
                </Grid>
                <Grid item xs={4} className={classes.gridItemValueLabel}>
                    <Switch
                        checked={props.onFunc}
                        onChange={() => handleChange(props.itemKey)}
                        color='primary'
                        name={props.itemKey}
                        size='medium'
                        inputProps={{ 'aria-label': props.itemKey }} />
                </Grid>
                <Grid item xs={4} className={classes.gridItemValueLabel}>
                    <Typography variant='body1'>{props.onLabel}</Typography>
                </Grid>
            </Grid>
        </Grid>
    )
}

export default BooleanItem;