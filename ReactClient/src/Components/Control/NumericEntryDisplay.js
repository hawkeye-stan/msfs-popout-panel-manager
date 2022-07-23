import React, { useState } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { useLocalStorageData } from '../../Services/LocalStorageProvider';
import { Typography } from '@mui/material';
import SevenSegmentDisplay from '../Control/SevenSegmentDisplay';
import NumPad from '../ControlDialog/NumPad';

const useStyles = makeStyles((theme) => ({
    segmentContainer: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center',
    },
    digitContainer: {
        ...theme.custom.sevenDigitPanelContainer,
        padding: '4px'
    },
    digitClickableContainer: {
        ...theme.custom.sevenDigitPanelClickableContainer,
        padding: '4px'
    },
    segmentDisplayLabel: {
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        fontSize: '0.4em',
        padding: '0em 1em 0em 1em',
        width: '3em'
    }
}));

const NumericEntryDisplay = ({ initialValue, labelLeft, labelRight, numberOfDigit, numberOfDisplayDigit, decimalPlaces, minValue, maxValue, disableNumPadKeys,
    onSelect, onSet, disableEntry, usedByArduino, onColor}) => {

    const classes = useStyles();
    const { isUsedArduino } = useLocalStorageData().configurationData;
    const [keyPadOpen, setKeyPadOpen] = useState(false);
    const [anchorEl, setAnchorEl] = useState(null);

    let disabled = (isUsedArduino && usedByArduino) || disableEntry;

    if(isNaN(initialValue)) initialValue = 0;

    const handleClose = () => {
        if (!disabled)
            setKeyPadOpen(!keyPadOpen);
    }

    const handleOnClick = (event) => {
        setAnchorEl(event.currentTarget);

        if (!disabled)
            setKeyPadOpen(!keyPadOpen);

        if (onSelect != null)
            onSelect(this, null);
    }

    const handleOnSet = (value) => {
        if (!disabled && onSet !== null)
            onSet(value);
    }

    return (
        <div>
            <div className={classes.segmentContainer} >
                {labelLeft !== null &&
                    <Typography className={classes.segmentDisplayLabel} variant='body1'>{labelLeft}</Typography>
                }
                <div className={disableEntry ? classes.digitContainer : classes.digitClickableContainer}
                    onClick={handleOnClick}>
                    <SevenSegmentDisplay
                        numberOfDigit={numberOfDigit}
                        numberOfDisplayDigit={numberOfDisplayDigit}
                        decimalPlaces={decimalPlaces}
                        value={initialValue}
                        onColor={onColor}>
                    </SevenSegmentDisplay>
                </div>
                {labelRight !== null &&
                    <Typography className={classes.segmentDisplayLabel} variant='body1'>{labelRight}</Typography>
                }
            </div>
            <NumPad
                anchorEl={anchorEl}
                open={keyPadOpen}
                onSet={handleOnSet}
                numberOfDigit={numberOfDigit}
                decimalPlaces={decimalPlaces}
                onClose={handleClose}
                minValue={minValue}
                maxValue={maxValue}
                disableNumPadKeys={disableNumPadKeys}>
            </NumPad>
          
        </div>
    )
}

NumericEntryDisplay.defaultProps = {
    initialValue: 0,
    labelLeft: null,
    labelRight: null,
    numberOfDigit: 0,
    numberOfDisplayDigit: 1,
    decimalPlaces: 0,
    minValue: 0,
    maxValue: 0,
    disabled: false,
    disableNumPadKeys: [],
    disableEntry: false,
    usedByArduino: true
};

export default NumericEntryDisplay;