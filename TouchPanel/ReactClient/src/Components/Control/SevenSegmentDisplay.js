import React from 'react';
import makeStyles from '@mui/styles/makeStyles';
import SevenSegmentDisplayDigit from './SevenSegmentDisplay/SevenSegmentDisplayDigit';

const useStyles = makeStyles(() => ({
    root: {
        flexGrow: 1,
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'flex-end',
        paddingRight: '0.1em'
    },
    panel: {
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'flex-end',
        alignContent: 'flex-start',
        alignItems: 'flex-end',
        padding: 0,
        margin: 0
    }
}));

const SevenSegmentDisplay = ({ onColor, offColor, numberOfDisplayDigit, decimalPlaces, decimalPlacesLessThanOneOnly, padZeroes, value, heightRatio, onClick }) => {
    const classes = useStyles();
    heightRatio = heightRatio / 2;

    const handleOnClick = () => {
        if (onClick != null)
            onClick(this, null);
    }

    const parseDigits = () => {
        let arr;

        if(value === '' || value === undefined)
            return [];
            
        if (decimalPlacesLessThanOneOnly && (Number(value) >= 1 || Number(value) === 0))
            arr = Array.from((Number(value).toFixed(0)));
        else
            arr = Array.from((Number(value).toFixed(decimalPlaces)));
   
        let numDisplay = arr.length > numberOfDisplayDigit ? arr.length : numberOfDisplayDigit;
        
        let charFill = padZeroes ? '0' : '';
        
        if(arr.includes('.'))
        {
            arr = Array(numDisplay + 1 - arr.length).fill(charFill, 0, numDisplay - arr.length).concat(arr);
            arr[numDisplay - decimalPlaces] = '.';
            return arr;
        }

        arr = Array(numDisplay - arr.length).fill(charFill, 0, numDisplay - arr.length).concat(arr);
        return arr;
    }

    return (
        <div className={classes.root} onClick={handleOnClick} >
            {parseDigits(value).map((v, index) => {
                return (
                    <div key={'digit' + index} className={classes.panel} style={{ marginLeft: Number((heightRatio / 2).toFixed(1)) + 'em' }}>
                        <SevenSegmentDisplayDigit value={v} onColor={onColor} offColor={offColor} heightRatio={heightRatio} />
                    </div>
                )
            })}
        </div>
    )
}

SevenSegmentDisplay.defaultProps = {
    heightRatio: 12,
    numberOfDisplayDigit: 1,
    decimalPlaces: 0,
    value: '',
    onColor: 'rgb(255, 255, 255, 1)',
    offColor: 'rgb(40, 44, 57, 0.4)'
};

export default SevenSegmentDisplay;