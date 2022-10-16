import React, { useState, useRef, useEffect, useMemo } from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { useSimConnectData } from '../Services/SimConnectDataProvider';
import { useLocalStorageData } from '../Services/LocalStorageProvider';
import KnobPadOverlay from '../Components/ControlDialog/KnobOverlay';
import InteractiveControlTemplate from './InteractiveControlTemplate';

const useStyles = makeStyles((theme) => ({
    root: {
        position: 'relative',
        width: '100%',
        height: '100%',
        margin: '0 auto'
    },
    backgroundOverlay:
    {
        position: 'relative',
        backgroundColor: theme.palette.background,
        backgroundImage: (props) => `url(/config/profiles/${props.parentRootPath}/${props.rootPath}/img/${props.backgroundImage})`,
        aspectRatio: (props) => `${props.panelSize.width}/${props.panelSize.height}`,
        width: (props) => `calc(${props.panelSize.width} / ${props.parentPanelSize.width} * ${props.scale} * 100vw)`,
        backgroundRepeat: 'no-repeat',
        backgroundSize: '100% 100%',
        zIndex: 1000
    },
    iframe: {
        width: '100%',
        height: '100%',
    }
}));

const PopoutPanelContainer = ({panelInfo, highLightedControlId, highlightedControlChanged}) => {
    const { simConnectSystemEvent } = useSimConnectData();
    const { isUsedArduino } = useLocalStorageData().configurationData;
    const classes = useStyles(panelInfo);
    const [reload, setReload] = useState(true);
    const [keyPadOpen, setKeyPadOpen] = useState(false);
    const [showDualKnob, setShowDualKnob] = useState(false);
    const [anchorEl, setAnchorEl] = useState(null);
    const knobPadTimeout = useRef();
    
    const handleKeyPadClose = () => {
        setKeyPadOpen(false);
    }

    const handleShowEncoder = (event, useDualEncoder) => {
        setShowDualKnob(useDualEncoder)
        
        setAnchorEl(event.currentTarget);
        if (!isUsedArduino)
            setKeyPadOpen(true);

        clearTimeout(knobPadTimeout.current);
        knobPadTimeout.current = setTimeout(() => setKeyPadOpen(false), 2000);
    }

    const handleOnKnobPadActivate = () => {
        clearTimeout(knobPadTimeout.current);
        knobPadTimeout.current = setTimeout(() => setKeyPadOpen(false), 2000);
    }

    useEffect(() =>{
        if(simConnectSystemEvent !== null)
        {
            if(simConnectSystemEvent === 'SIMSTART')
                setReload(true);
            else if(simConnectSystemEvent === 'SIMSTOP')
                setReload(false);
        }
    }, [simConnectSystemEvent])

    
    return useMemo(() => (
        <div className={classes.root}>
            { reload &&
                <div className={classes.backgroundOverlay}>
                    { Array.isArray(panelInfo.definitions) && panelInfo.definitions !== undefined && panelInfo.definitions.map(ctrl =>
                        <InteractiveControlTemplate 
                            key={ctrl.id} 
                            ctrl={ctrl}
                            panelInfo={panelInfo}
                            showEncoder={(e, useDualEncoder) => handleShowEncoder(e, useDualEncoder)}
                            highLightedControlId={highLightedControlId}
                            highlightedControlChanged={(ctrlId) => highlightedControlChanged(ctrlId)}
                            >
                        </InteractiveControlTemplate>
                    )}
                </div>
            }
            {!isUsedArduino && keyPadOpen &&
                <KnobPadOverlay
                    open={keyPadOpen}
                    onClose={handleKeyPadClose}
                    showDualKnob={showDualKnob}
                    anchorEl={anchorEl}
                    onKnobPadActivate={() => handleOnKnobPadActivate()}>
                </KnobPadOverlay>
            }
        </div>
    ), [panelInfo, keyPadOpen, isUsedArduino, highLightedControlId])
}

export default PopoutPanelContainer;