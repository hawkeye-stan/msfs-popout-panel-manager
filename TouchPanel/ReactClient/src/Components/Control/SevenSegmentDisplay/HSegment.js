import React from 'react';
import Arrow from './Arrow';

const HSegment = ({ width, height, color }) => {
    return (
        <div
            style={{
                position: 'relative',
                margin: '0em auto',
                width: width + 'em',
                height: height + 'em',
                backgroundColor: color
            }}>
            <Arrow size={height} color={color} direction={'left'} />
            <Arrow size={height} color={color} direction={'right'} />
        </div>
    )
}

export default HSegment;