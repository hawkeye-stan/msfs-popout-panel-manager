import React, { useEffect, useState } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { Typography } from '@mui/material'
import Grid from '@mui/material/Grid';
import Button from '@mui/material/Button';
import Popover from '@mui/material/Popover';
import IconButton from '@mui/material/IconButton';
import InfoIcon from '@mui/icons-material/Info';
import BackspaceIcon from '@mui/icons-material/Backspace';
import CheckIcon from '@mui/icons-material/Check';
import CloseIcon from '@mui/icons-material/Close';
import Tooltip from '@mui/material/Tooltip';

const useStyles = makeStyles((theme) => ({
    dialog: {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        touchAction: 'none'
    },
    paper: {
        width: '270px',
        ...theme.custom.defaultDialog
    },
    controlBar: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        height: '3em',
        padding: '0 8px'
    },
    numericDisplay: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        height: '3em',
        background: 'transparent',
        border: '1px solid ' + theme.palette.border,
        padding: '0 8px 0 16px'
    },
    numericValue: {
        display: 'flex',
        justifyContent: 'flex-end',
        flexGrow: 1,
        paddingRight: '8px'
    },
    keypadBox: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'space-between',
        padding: 0,
        margin: 0
    },
    keypadKey:{
        padding: 0
    },
    keypadButton: {
        border: '1px solid ' + theme.palette.border,
        width: '100%',
        borderRadius: 0,
        padding: 0,
        backgroundColor: 'transparent',
        [theme.breakpoints.up('sm')]: { height: '3.5em' },
        [theme.breakpoints.up('md')]: { height: '5em' }
    },
    keypadButtonZero: {
        width: '100%',
        height: '5em',
        border: '1px solid ' + theme.palette.border,
        padding: 0,
        backgroundColor: 'transparent',
        [theme.breakpoints.up('sm')]: { height: '3.5em' },
        [theme.breakpoints.up('md')]: { height: '5em' }
    },
    directInputSwitch: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center'
    }
}));

const NumPad = ({ anchorEl, open, numberOfDigit, decimalPlaces, minValue, maxValue, disableNumPadKeys, onSet, onClose }) => {
    const classes = useStyles();
    const [keyPadValue, setKeyPadValue] = useState('');
    const [isValid, setIsValid] = useState(false);

    useEffect(() => {
        if (open)
            setKeyPadValue('')
    }, [open]);

    const handleClose = () => {
        if (onClose !== undefined)
            onClose();
    }

    const handleKeyPress = (value) => {
        let digitCount = keyPadValue.substr(0, 1) === '-' ? numberOfDigit + 1 : numberOfDigit;

        if (keyPadValue.replace('.', '').length < digitCount)
            addDigit(value);
    }

    const handleBackspace = () => {
        if (keyPadValue !== 0)
            removeDigit();
    }

    const handleOnSet = () => {
        if (isValid && onSet !== undefined)
            onSet(Number(keyPadValue));

        handleClose();
    }

    const addDigit = (digit) => {
        let newValue = keyPadValue.replace('.', '') + digit;

        let isNegative = (newValue.substr(0, 1) === '-');

        if (isNegative)
            newValue = newValue.substr(1);

        if (decimalPlaces > 0 && newValue.length > decimalPlaces) {
            let decIndex = newValue.length - decimalPlaces;
            newValue = newValue.substr(0, decIndex) + '.' + newValue.substr(decIndex);
        }

        if (isNegative)
            newValue = '-' + newValue;

        setKeyPadValue(newValue);
        checkValid(newValue)

    }

    const removeDigit = () => {
        let newValue = keyPadValue.replace('.', '').slice(0, -1);

        let isNegative = (newValue.substr(0, 1) === '-');

        if (isNegative)
            newValue = newValue.substr(1);

        if (decimalPlaces > 0 && newValue.length > decimalPlaces) {
            let decIndex = newValue.length - decimalPlaces;
            newValue = newValue.substr(0, decIndex) + '.' + newValue.substr(decIndex);
        }

        if (isNegative)
            newValue = '-' + newValue;

        setKeyPadValue(newValue);
        checkValid(newValue)
    }

    const checkValid = (value) => {
        let valid = Number(value) >= minValue
            && Number(value) <= maxValue
            && value !== '';
        setIsValid(valid);
    }

    return (
        <Popover 
            aria-labelledby='KnobPad' 
            aria-describedby='KnobPad' 
            anchorEl={anchorEl} 
            anchorOrigin={{
                vertical: 'bottom',
                horizontal: 'left',
                }}
            transformOrigin={{
                vertical: 'top',
                horizontal: 'right',
            }}
            className={classes.dialog} 
            open={open} 
            onClose={handleClose}
        >
            <div className={classes.paper}>
                <div className={classes.controlBar}>
                    <IconButton color='inherit' aria-label='close' size='medium' onClick={handleClose}>
                        <CloseIcon />
                    </IconButton>
                    <IconButton color='inherit' aria-label='accept' size='medium' disabled={!isValid} onClick={handleOnSet}>
                        <CheckIcon />
                    </IconButton>
                </div>
                <div className={classes.numericDisplay}>
                    <Tooltip title={'Must be between ' + minValue + ' and ' + maxValue} placement="top" >
                        <InfoIcon />
                    </Tooltip>
                    <div className={classes.numericValue}>
                        <Typography variant='h2'>{String(keyPadValue).replace(/^0\.[0]*/, '')}</Typography>
                    </div>
                    <IconButton color='inherit' aria-label='backspace' size='medium' onClick={handleBackspace}>
                        <BackspaceIcon />
                    </IconButton>
                </div>

                <div>
                    <Grid container className={classes.keypadBox}>
                        {[1, 2, 3, 4, 5, 6, 7, 8, 9, '-', 0].map((v, index) => {
                            return (
                                <Grid item xs={v === 0 ? 8 : 4} key={'key' + index} className={classes.keypadKey}>
                                    <Button disabled={disableNumPadKeys.includes(v)} variant='outlined' className={v === 0 ? classes.keypadButtonZero : classes.keypadButton} onClick={() => handleKeyPress(v)}>
                                        <Typography variant='h2'>{v}</Typography>
                                    </Button>
                                </Grid>
                            )
                        })}
                    </Grid>
                </div>
            </div>
        </Popover>
    );
}

NumPad.defaultProps = {
    open: false,
    numberOfDigit: 0,
    decimalPlaces: 0,
    checkLength: true,
    minValue: 0,
    maxValue: 1000,
    disableNumPadKeys: []
};

export default NumPad;
