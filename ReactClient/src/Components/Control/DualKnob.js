import React, { useState } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import PropTypes from 'prop-types';
import { Basic } from 'react-dial-knob'
import IconButton from '@mui/material/IconButton';

// FIXME checkout https://mui.com/components/use-media-query/#migrating-from-withwidth
const withWidth = () => (WrappedComponent) => (props) => <WrappedComponent {...props} width="xs" />;

const useStyles = makeStyles((theme) => ({
    root: {
        touchAction: 'none'
    },
    knobSection: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center',
    },
    knob: {
        position: 'absolute',
        '& svg': {
            '& g': {
                '& circle:last-child': {
                    fill: '#777777'
                    //display: 'none'       // hide the knob wheel's dot
                },
                '& circle:nth-child(2)': {
                    strokeWidth: '1'
                }
            }
        },
        '& div': {
            '& div': {
                '& div': {
                    display: 'none'
                }
            }
        }
    },
    knobButton: {
        position: 'absolute',
    }
}));

const MIN_DIAL_STEP = 1;
const MAX_DIAL_STEP = 10;
const KNOB_THEME = {
    defaultColor: '#FFFFFF',
    activeColor: '#A8A8A8',
    gradientStart: '#E8E8E8',
    gradientEnd: '#A8A8A8'
}

const DualKnob = ({ width, onUpperKnobIncrease, onUpperKnobDecrease, onLowerKnobIncrease, onLowerKnobDecrease, onKnobPush, showDualKnob, showKnobButton}) => {
    const classes = useStyles();
    const [upperKnob, setUpperKnob] = useState();
    const [lowerKnob, setLowerKnob] = useState();

    const KnobChanged = (knobValue, knob) => {
        let isIncrease;

        // determin if turning knob to increase or decrease data value
        if (knobValue === MIN_DIAL_STEP && knob === MAX_DIAL_STEP)
            isIncrease = true;
        else if (knobValue === MAX_DIAL_STEP && knob === MIN_DIAL_STEP)
            isIncrease = false;
        else if (knobValue > knob)
            isIncrease = true;
        else if (knobValue < knob)
            isIncrease = false;
        else
            return;

        return isIncrease;
    }

    const handleUpperKnobChanged = (knobValue) => {
        setUpperKnob(knobValue);

        if (upperKnob !== undefined) {
            KnobChanged(knobValue, upperKnob) ? raiseEvent(onUpperKnobIncrease) : raiseEvent(onUpperKnobDecrease);
        }
    }

    const handleLowerKnobChanged = (knobValue) => {
        setLowerKnob(knobValue);

        if (lowerKnob !== undefined) {
            KnobChanged(knobValue, lowerKnob) ? raiseEvent(onLowerKnobIncrease) : raiseEvent(onLowerKnobDecrease);
        }
    }

    const handleKnobPress = () => {
        raiseEvent(onKnobPush);
    }

    const raiseEvent = (event) => {
        if (event != null)
            event();
    }

    return (
        <div className={classes.root}>
            <div className={classes.knobSection}>
                <div className={classes.knob}>
                    <Basic
                        diameter={width === 'sm' ? 180 : (showDualKnob ? 250: 180)}
                        min={MIN_DIAL_STEP}
                        max={MAX_DIAL_STEP}
                        step={1}
                        theme={KNOB_THEME}
                        onValueChange={(value) => { handleLowerKnobChanged(value) }}
                        value={lowerKnob}
                        ariaLabelledBy={'lowerknob'}>
                    </Basic>
                </div>
                {showDualKnob &&
                    <div className={classes.knob}>
                        <Basic
                            diameter={width === 'sm' ? 70 : 140}
                            min={MIN_DIAL_STEP}
                            max={MAX_DIAL_STEP}
                            step={1}
                            theme={KNOB_THEME}
                            onValueChange={(value) => { handleUpperKnobChanged(value) }}
                            value={upperKnob}
                            ariaLabelledBy={'upperknob'}>
                        </Basic>
                    </div>
                }
                { showKnobButton && 
                    <div className={classes.knobButton}>
                        <IconButton onClick={handleKnobPress} style={{width: width === 'sm' ? 40 : 60, height: width === 'sm' ? 40 : 60, border: '1px solid #A8A8A8'}}></IconButton >
                    </div>
                }
            </div>
        </div>
    )
}

DualKnob.propTypes = {
    width: PropTypes.oneOf(['lg', 'md', 'sm', 'xl', 'xs']).isRequired,
};

export default withWidth()(DualKnob);