import React from 'react';
import makeStyles from '@mui/styles/makeStyles';
import DualKnob from '../Control/DualKnob';
import Popover from '@mui/material/Popover';
import { simActions } from '../../Services/simConnectActionHandler';

const useStyles = makeStyles((theme) => ({
    dialog: {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        touchAction: 'none',
        backgroundColor: 'transparent',
        opacity: 0.5,
        '& .MuiPopover-paper':
        {
            backgroundImage: 'none !important',
            backgroundColor: 'transparent',
            border: '0 !important',
            boxShadow: 'none'
        }
    },
    paper: {
        ...theme.custom.defaultDialog,
        width: '255px',
        height: '255px',
        backgroundColor: 'transparent',
        border: '1px solid transparent'
    },
    controlBar: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        height: '3em',
        padding: '0 8px'
    },
    directInputSwitch: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center'
    },
    numericDisplay: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center',
        background: 'transparent',
        border: '1px solid ' + theme.palette.border
    },
    knob: {
        [theme.breakpoints.up('sm')]: { marginTop: '100px' },
        [theme.breakpoints.up('md')]: { marginTop: '125px' }
    },

}));

const KnobOverlay = ({ open, onClose, anchorEl, showDualKnob = false, onKnobPadActivate }) => {
    const classes = useStyles();

    const handleClose = () => {
        if (onClose !== undefined)
            onClose();
    }

    const HandleUpperKnobIncrease = () => {
        simActions.Encoder.upperIncrease();
        onKnobPadActivate();
    }

    const HandleUpperKnobDecrease = () => {
        simActions.Encoder.upperDecrease();
        onKnobPadActivate();
    }

    const HandleLowerKnobIncrease = () => {
        simActions.Encoder.lowerIncrease();
        onKnobPadActivate();
    }

    const HandleLowerKnobDecrease = () => {
        simActions.Encoder.lowerDecrease();
        onKnobPadActivate();
    }

    const HandleEncoderPush = () => {
        simActions.Encoder.push();
        onKnobPadActivate();
    }

    return (
        <Popover 
            aria-labelledby='KnobPad' 
            aria-describedby='KnobPad' 
            anchorEl={anchorEl} 
            anchorOrigin={{
                vertical: 'center',
                horizontal: 'center',
                }}
            transformOrigin={{
                vertical: 'center',
                horizontal: 'center',
            }}
            className={classes.dialog} 
            open={open} 
            onClose={handleClose}>
           
            <div className={classes.paper}>
                <div className={classes.knob}>
                    <DualKnob
                        onUpperKnobIncrease={HandleUpperKnobIncrease}
                        onUpperKnobDecrease={HandleUpperKnobDecrease}
                        onLowerKnobIncrease={HandleLowerKnobIncrease}
                        onLowerKnobDecrease={HandleLowerKnobDecrease}
                        onKnobPush={HandleEncoderPush}
                        showKnobButton={true}
                        showDualKnob={showDualKnob}>
                    </DualKnob>
                </div>
            </div>
        </Popover>
    )
}

export default KnobOverlay;