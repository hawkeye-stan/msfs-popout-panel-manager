import React, {useEffect, useState} from 'react';
import makeStyles from '@mui/styles/makeStyles';
import { useHistory } from 'react-router-dom';
import ListSubheader from '@mui/material/ListSubheader';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Collapse from '@mui/material/Collapse';
import Divider from '@mui/material/Divider';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import FlightIcon from '@mui/icons-material/Flight';
import SensorWindowIcon from '@mui/icons-material/SensorWindow';
import { simConnectGetPlanePanelProfilesInfo }  from '../Services/SimConnectDataProvider';

const useStyles = makeStyles((theme) => ({
    rootFullWidth: {
        [theme.breakpoints.up('sm')]: { fontSize: '12px' },
        [theme.breakpoints.up('md')]: { fontSize: '16px' },
        [theme.breakpoints.up('lg')]: { fontSize: '18px' },
        [theme.breakpoints.up('xl')]: { fontSize: '18px' },
        padding: 0,
        maxWidth: '100vw',
        display: 'grid',
        height: '100vh',
        backgroundColor: 'black',
        overflow: 'auto'
    }
}));

const PlaneProfileList = ({plane}) => {
    const history = useHistory();
    const [open, setOpen] = React.useState(false);

    return (
        <>
            <Divider></Divider>
            <ListItemButton onClick={() => setOpen(!open)}>
                <ListItemIcon><FlightIcon /></ListItemIcon> 
                <ListItemText primary={plane.name} />
                {open ? <ExpandLess /> : <ExpandMore />}
            </ListItemButton>
            <Collapse in={open} timeout="auto" unmountOnExit>
                {plane.panels.map((panel) => 
                    <List key={panel.panelId} component="div" onClick={() => history.push(`/${plane.planeId}/${panel.panelId}`)}>
                        <ListItemButton sx={{ pl: 4 }}>
                            <ListItemIcon><SensorWindowIcon /></ListItemIcon>
                            <ListItemText primary={panel.name} />
                        </ListItemButton>
                    </List>
                )}
            </Collapse>
        </>
    )
}

const WebPanelSelection = () => {
    const classes = useStyles();
    const [ planePanelProfileInfo, setPlanePanelProfileInfo ] = useState();

    useEffect(async() => {
        let data = await simConnectGetPlanePanelProfilesInfo();

        if(data !== null)
            setPlanePanelProfileInfo(data);
        
    }, []);


    return (
        <div className={classes.rootFullWidth}>
            <List component="nav" subheader={
                <ListSubheader component="div" id="nested-list-subheader">
                    Please select a plane profile to open the corresponding panel
                </ListSubheader>
            }>
                { planePanelProfileInfo !== undefined && planePanelProfileInfo.map((plane) => 
                    <PlaneProfileList key={plane.planeId} plane={plane}></PlaneProfileList>
                )}
            </List>
        </div>
    );
}

export default WebPanelSelection

