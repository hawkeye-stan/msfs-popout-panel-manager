import React from 'react';
import HSegment from './HSegment';
import VSegment from './VSegment';

// Segment map uses bit based algorithm
// References: http://www.uize.com/examples/seven-segment-display.html

const bitReadAll = (value) => {
    return [6, 5, 4, 3, 2, 1, 0].map(function (bit) { return Boolean((value >> bit) & 0x01); });
}

const segmentMap = {
    0: bitReadAll(0x7e),
    1: bitReadAll(0x30),
    2: bitReadAll(0x6d),
    3: bitReadAll(0x79),
    4: bitReadAll(0x33),
    5: bitReadAll(0x5b),
    6: bitReadAll(0x5f),
    7: bitReadAll(0x70),
    8: bitReadAll(0x7f),
    9: bitReadAll(0x7b),
    '_': bitReadAll(0x08),
    '': bitReadAll(0x00),
    ' ': bitReadAll(0x00),
    '-': bitReadAll(0x01)
}

const SevenSegmentDisplayDigit = ({ onColor, offColor, heightRatio, value }) => {
    let width;
    let height;
    if(heightRatio < 0.3) {
        width = Number((heightRatio).toFixed(2));
        height = Number((heightRatio / 5).toFixed(2));
    }
    else {
        width = Number((heightRatio).toFixed(1));
        height = Number((heightRatio / 5).toFixed(1));
    }
   
    if (!isNaN(value) && value !== '' && value !== ' ') value = Number(value);

    const [a, b, c, d, e, f, g] = value in segmentMap ? segmentMap[value] : bitReadAll(Number(value));

    if (value === '.') {
        return (
            <div
                style={{
                    display: 'inline-block'
                }}>
                <HSegment width={width / 4} height={height} color={onColor} />
            </div>
        )
    }

    return (
        <div
            style={{
                display: 'inline-block',
                width: (width + (height * 2)).toFixed(1) + 'em'
            }}>
            <HSegment width={width} height={height} color={a ? onColor : offColor} />
            <div style={{ position: 'relative', width: '100%', height: `${width}em` }}>
                <VSegment width={height} height={width} color={f ? onColor : offColor} align="left" />
                <VSegment width={height} height={width} color={b ? onColor : offColor} align="right" />
            </div>
            <HSegment width={width} height={height} color={g ? onColor : offColor} />
            <div style={{ position: 'relative', width: '100%', height: `${width}em` }}>
                <VSegment width={height} height={width} color={e ? onColor : offColor} align="left" />
                <VSegment width={height} height={width} color={c ? onColor : offColor} align="right" />
            </div>
            <HSegment width={width} height={height} color={d ? onColor : offColor} />
        </div>
    )
}

SevenSegmentDisplayDigit.defaultProps = {
    value: '',
    onColor: 'rgb(32, 217, 32, 1)',
    offColor: 'rgb(40, 44, 57, 0.3)'
};

export default SevenSegmentDisplayDigit;