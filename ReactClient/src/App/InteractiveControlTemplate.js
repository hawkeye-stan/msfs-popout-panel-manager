import React, { useMemo, useRef, useEffect } from 'react';
import IconButton from '@mui/material/IconButton';
import makeStyles from '@mui/styles/makeStyles';
import { useSimConnectData } from '../Services/SimConnectDataProvider';
import { useLocalStorageData } from '../Services/LocalStorageProvider';
import { simConnectPost } from '../Services/simConnectPost';
import { useWindowDimensions } from '../Components/Util/hooks';
import { Typography } from '@mui/material';
import SevenSegmentDisplay from '../Components/Control/SevenSegmentDisplay';

const useStyles = makeStyles(() => ({
    iconButton: {
        width: '100%',
        height: '100%'
    },
    iconImageHighlight: {
        filter: 'sepia(80%)'
    },
    controlBase: {
        position: 'absolute',
        backgroundRepeat: 'no-repeat',
        backgroundSize: '100%',
        paddingRight: '0.1em'
    }
}));

const getImagePath = (panelInfo) => {
    return `/config/profiles/${panelInfo.parentRootPath}/${panelInfo.rootPath}/img/`;
}

const playSound = (isEnabledSound) => {
    if (isEnabledSound) {
        let audio = new Audio('/sound/button-click.mp3');
        audio.play();
    }
}

const execActions = (event, action, simConnectData, showEncoder) => {
    for (let i = 0; i < action.touchActions.length; i++) {
        let curAction = action.touchActions[i];
        let actionValue;

        if (curAction.actionValueVariable !== undefined)
            actionValue = simConnectData[curAction.actionValueVariable];
        else
            actionValue = curAction.actionValue === undefined ? 1 : curAction.actionValue

        if (curAction.action !== undefined && curAction.action !== null)
            setTimeout(() =>
                simConnectPost({
                    action: curAction.action,
                    actionValue: actionValue,
                    actionType: curAction.actionType,
                    encoderAction: action.encoderAction === undefined ? null : action.encoderAction
                }), 200 * i);
            

            if(action.useEncoder)
            {
                showEncoder(event, false);
                return;
            }

            if(action.useDualEncoder)
                showEncoder(event, action.useDualEncoder);
    }
}

const setupControlLocationStyle = (ctrl, panelInfo) => {
    if(panelInfo === undefined || panelInfo === null)
        return {};
    
    return { left: (ctrl.left / panelInfo.panelSize.width * 100.0) + '%', top: (ctrl.top / panelInfo.panelSize.height * 100.0) + '%' };
}

const setupControlWidthHeightStyle = (ctrl, panelInfo) => {
    if (ctrl.controlSize === undefined)
        return;

    return { width: `calc(100% * ${ctrl.controlSize.width} / ${panelInfo.panelSize.width})`, height: `calc(100% * ${ctrl.controlSize.height} / ${panelInfo.panelSize.height})` };
}

const ImageControl = ({ctrl, panelInfo}) => {
    const classes = useStyles();
    const imagePath = getImagePath(panelInfo);
    const isHighlighted = useRef(false);

    const setupBackgroundImageStyle = () => {
        if (ctrl.image === undefined) {
            console.log('Missing image value for image control... Button Id: ' + ctrl.id);
            return {};
        }

        return { backgroundImage: `url(${imagePath}${ctrl.image})` };
    }

    return useMemo(() =>(
        <div className={[classes.controlBase, ctrl.highlight && isHighlighted.current ? classes.iconImageHighlight : ''].join(' ')} style={{ ...setupControlLocationStyle(ctrl, panelInfo), ...setupBackgroundImageStyle(ctrl, panelInfo), ...setupControlWidthHeightStyle(ctrl, panelInfo) }}>
            <IconButton className={classes.iconButton} />
        </div>
    ), [ctrl, isHighlighted.current])
}

const ImageButton = ({ctrl, panelInfo, showEncoder}) => {
    const classes = useStyles();
    const { simConnectData } = useSimConnectData();
    const { isUsedArduino, isEnabledSound } = useLocalStorageData().configurationData;
    const imagePath = getImagePath(panelInfo);
    const isHighlighted = useRef(false);

    const setupBackgroundImageStyle = () => {
        if (ctrl.image === undefined) {
            console.log('Missing image value for image button control... Button Id: ' + ctrl.id);
            return {};
        }

        return { backgroundImage: `url(${imagePath}${ctrl.image})` };
    }

    const handleOnClick = (event) => {
        if (ctrl.highlight === undefined || ctrl.highlight) {
            isHighlighted.current = true;
            setTimeout(() => { isHighlighted.current = false; }, 2000);
        }

        if (ctrl.actions != null && ctrl.actions.length > 0)
            playSound(isEnabledSound);

        if (!isUsedArduino && (ctrl.action.useEncoder || ctrl.action.useDualEncoder))
            showEncoder(event, ctrl.action.useDualEncoder === undefined ? false : ctrl.action.useDualEncoder);

        execActions(event, ctrl.action, simConnectData, showEncoder);
    }

    return useMemo(() =>(
        <div className={[classes.controlBase, ctrl.highlight && isHighlighted.current ? classes.iconImageHighlight : ''].join(' ')} style={{ ...setupControlLocationStyle(ctrl, panelInfo), ...setupBackgroundImageStyle(ctrl, panelInfo), ...setupControlWidthHeightStyle(ctrl, panelInfo) }}>
            <IconButton className={classes.iconButton} onClick={(event) => handleOnClick(event)} />
        </div>
    ), [ctrl, isHighlighted.current, isUsedArduino, isEnabledSound])
}

const BindableImageButton = ({ctrl, panelInfo, showEncoder}) => {
    const classes = useStyles();
    const { simConnectData } = useSimConnectData();
    const { isUsedArduino, isEnabledSound } = useLocalStorageData().configurationData;
    const imagePath = getImagePath(panelInfo);
    const isHighlighted = useRef(false);
    const dataBindingValue = simConnectData[ctrl.binding?.variable];

    const setupBackgroundImageStyle = () => {
        if (ctrl.binding !== undefined && ctrl.binding.images !== undefined) {
            let image = ctrl.binding.images.find(x => x.val === dataBindingValue);

            if(dataBindingValue === undefined)
            {
                image = ctrl.binding.images[0]; 
            }
            else if (image === undefined && dataBindingValue !== undefined)
            {
                console.log('Missing binding value for bindable image button control... Button Id: ' + ctrl.id + ' Data Value: ' + dataBindingValue);
                image = ctrl.binding.images[0];
            }

            if (image.rotate !== undefined)
                return { backgroundImage: `url(${imagePath}${image.url})`, transform: `rotate(${image.rotate}deg)` };
            else
                return { backgroundImage: `url(${imagePath}${image.url})` };
        }

        return {};
    }

    const handleOnClick = (event) => {
        if (ctrl.highlight === undefined || ctrl.highlight) {
            isHighlighted.current = true;
            setTimeout(() => { isHighlighted.current = false; }, 2000);
        }

        if (ctrl.actions != null && ctrl.actions.length > 0)
            playSound(isEnabledSound);

        if (!isUsedArduino && (ctrl.action.useEncoder || ctrl.action.useDualEncoder))
            showEncoder(event, ctrl.action.useDualEncoder === undefined ? false : ctrl.action.useDualEncoder);

        execActions(event, ctrl.action, simConnectData, showEncoder);
    }

    return useMemo(() =>(
        <div className={[classes.controlBase, ctrl.highlight && isHighlighted.current ? classes.iconImageHighlight : ''].join(' ')} style={{ ...setupControlLocationStyle(ctrl, panelInfo), ...setupBackgroundImageStyle(ctrl, panelInfo, dataBindingValue), ...setupControlWidthHeightStyle(ctrl, panelInfo) }}>
            <IconButton className={classes.iconButton} onClick={(event) => handleOnClick(event)} />
        </div>
    ), [ctrl, dataBindingValue, isHighlighted.current, isUsedArduino, isEnabledSound])
}

const DigitDisplay = ({ctrl, panelInfo}) => {
    const classes = useStyles();
    const windowHeight = useWindowDimensions().windowHeight;
    const dataBindingValue = useSimConnectData().simConnectData[ctrl.binding?.variable];

    return useMemo(() =>(
        <div className={classes.controlBase} style={{ ...setupControlLocationStyle(ctrl, panelInfo), ...setupControlWidthHeightStyle(ctrl, panelInfo) }}>
            <SevenSegmentDisplay
                numberOfDisplayDigit={ctrl.binding.numberOfDisplayDigit}
                fontSizeInEm={ctrl.binding.fontSizeInEm}
                decimalPlaces={ctrl.binding.decimalPlaces === undefined ? 0 : ctrl.binding.decimalPlaces}
                decimalPlacesLessThanOneOnly={ctrl.binding.decimalPlacesLessThanOneOnly === undefined ? false : ctrl.binding.decimalPlacesLessThanOneOnly}
                padZeroes={ctrl.binding.padZeroes}
                onColor={ctrl.binding.color === null ? 'rgb(255, 255, 255, 1' : ctrl.binding.color}
                heightRatio={(windowHeight / panelInfo.parentPanelSize.height * panelInfo.scale)}
                value={dataBindingValue}>
            </SevenSegmentDisplay>
        </div>
    ), [ctrl, dataBindingValue, windowHeight])
}

const WebBrowser = ({ctrl, panelInfo}) => {
    const classes = useStyles();

    return useMemo(() =>(
        <div className={classes.controlBase} style={{ ...setupControlLocationStyle(ctrl, panelInfo), ...setupControlWidthHeightStyle(ctrl, panelInfo) }}>
            <iframe style={{ width: '100%', height: '100%', border: 'none' }} src={ctrl.binding.url} title="panel" />
        </div>
    ), [ctrl])
}

const TextBlock = ({ctrl, panelInfo}) => {
    const classes = useStyles();
    const dataBindingValue = useSimConnectData().simConnectData[ctrl.binding?.variable];

    const getTextBlockBindingValue = (ctrl) => {
        if (ctrl.binding !== undefined && ctrl.binding.variable !== undefined && ctrl.binding.bindingValues !== undefined) {
            var bindingValue = ctrl.binding.bindingValues.find(x => x.val === dataBindingValue);

            if (bindingValue !== undefined)
                return bindingValue.text;

            return null;
        }

        return ctrl.binding.text;
    }

    return useMemo(() =>(
        <div className={classes.controlBase} style={{ ...setupControlLocationStyle(ctrl, panelInfo), ...setupControlWidthHeightStyle(ctrl, panelInfo) }}>
            <Typography color={ctrl.binding.color === null ? 'rgb(255, 255, 255, 1' : ctrl.binding.color}>{getTextBlockBindingValue(ctrl)}</Typography>
        </div>
    ), [ctrl, dataBindingValue])
}


const InteractiveControlTemplate = ({ ctrl, panelInfo, showEncoder }) => {
    // preload all dynamic bindable images for control
    useEffect(() => {
        let imagePath = getImagePath(panelInfo);

        if(ctrl?.binding?.images !== undefined)
        {
            ctrl.binding.images.forEach((p)=>{
                let img = new Image();
                img.src = imagePath + p.url;
                img.onload = () => {};
            });
        }
    }, [])

    return (
        <>
            {ctrl.type === 'image' && 
                <ImageControl ctrl={ctrl} panelInfo={panelInfo}></ImageControl>
            }
            {ctrl.type === 'imageButton' && 
                <ImageButton ctrl={ctrl} panelInfo={panelInfo} showEncoder={showEncoder}></ImageButton>
            }
            {ctrl.type === 'bindableImageButton' && 
                <BindableImageButton ctrl={ctrl} panelInfo={panelInfo} showEncoder={showEncoder}></BindableImageButton>
            }
            {ctrl.type === 'digitDisplay' && 
                <DigitDisplay ctrl={ctrl} panelInfo={panelInfo}></DigitDisplay>
            }
            {ctrl.type === 'webBrowser' && 
                <WebBrowser ctrl={ctrl} panelInfo={panelInfo}></WebBrowser>
            }
            {ctrl.type === 'textBlock' && 
                <TextBlock ctrl={ctrl} panelInfo={panelInfo}></TextBlock>
            }
        </>
    )
}

export default InteractiveControlTemplate;
