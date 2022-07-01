import React, { useState, useRef } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import Button from '@mui/material/Button';

const useStyles = makeStyles({
    root: {
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'left',
        alignItems: 'center'
    },
    displayActive: {
        fontSize: '0.7em',
        color: 'rgb(32, 217, 32, 1)',
        marginRight: '0.5em',
        
    },
    displayInactive: {
        fontSize: '0.7em',
        color: 'rgb(255, 255, 255, 1)',
        marginRight: '0.5em'
    },
    button: {
        width: '5em',
        minHeight: '3em',
        fontSize: '0.5em',
        padding: 0,
        margin: '0 0 0 1.5em'
    }
});

const StopWatch = () => {
    const classes = useStyles();
    const [startLabel, setStartLabel] = useState('Start');
    const [isActive, setIsActive] = useState(false)
    const [timer, setTimer] = useState(0);
    const timerInterval = useRef(null);

    const handleOnStart = () => {
        if (isActive) {
            setStartLabel('Start');
            clearInterval(timerInterval.current);
            setIsActive(false);
        }
        else {
            setStartLabel('Stop')
            setIsActive(true);
            timerInterval.current = setInterval(() => {
                setTimer((timer) => timer + 1);
            }, 1000);
        }
    }

    const handleOnReset = () => {
        clearInterval(timerInterval.current);
        setTimer(0);
        setIsActive(false);
        setStartLabel('Start');
    }

    const formatTime = () => {
        const getSeconds = `0${(timer % 60)}`.slice(-2)
        const minutes = `${Math.floor(timer / 60)}`
        const getMinutes = `0${minutes % 60}`.slice(-2)
        const getHours = `0${Math.floor(timer / 3600)}`.slice(-2)

        return `${getHours} : ${getMinutes} : ${getSeconds}`
    }

    return (
        <div className={classes.root}>
            <div className={isActive ? classes.displayActive : classes.displayInactive}>{formatTime()}</div>
            <Button className={classes.button}
                variant="contained"
                size='x-small'
                color='primary'
                onClick={() => handleOnStart()}>
                {startLabel}
            </Button>
            <Button className={classes.button}
                variant="contained"
                size='small'
                color='primary'
                onClick={() => handleOnReset()}>
                Reset
            </Button>
        </div>
    )
}

export default StopWatch;


