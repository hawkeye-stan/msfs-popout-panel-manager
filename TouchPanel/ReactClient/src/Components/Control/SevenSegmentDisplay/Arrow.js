import React from 'react';

const INVERSE_DIRECTION = {
    left: 'right',
    right: 'left',
    top: 'bottom',
    bottom: 'top'
}

const BORDER_COLOR_PROP = {
    left: 'borderLeftColor',
    right: 'borderRightColor',
    top: 'borderTopColor',
    bottom: 'borderBottomColor'
}

const Arrow = (props) => {
    return (
        <div
            style={{
                position: 'absolute',
                width: 0,
                height: 0,
                borderStyle: 'solid',
                flexShrink: 1,
                borderRightColor: 'transparent',
                borderLeftColor: 'transparent',
                borderTopColor: 'transparent',
                borderBottomColor: 'transparent',
                borderWidth: props.size * 0.25 + 'em',
                [BORDER_COLOR_PROP[INVERSE_DIRECTION[props.direction]]]: props.color,
                [INVERSE_DIRECTION[props.direction]]: '100%'
            }
            }
        />
    )
}

export default Arrow;