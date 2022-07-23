import React from 'react'
import Arrow from './Arrow'

const VSegment = ({ width, height, color, align }) => {
    return (
        <div
            style={{
                position: 'absolute',
                width: width + 'em',
                height: height + 'em',
                backgroundColor: color,
                left: align === 'left' ? '0em' : '',
                right: align === 'right' ? '0em' : ''
            }}>
            <Arrow size={width} color={color} direction={'top'} />
            <Arrow size={width} color={color} direction={'bottom'} />
        </div>
    )
}

export default VSegment;
