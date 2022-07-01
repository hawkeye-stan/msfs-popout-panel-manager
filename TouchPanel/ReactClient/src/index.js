import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter, Route } from 'react-router-dom';
import { ThemeProvider, StyledEngineProvider } from '@mui/material/styles';
import { darkTheme } from './themes';
import './index.css';
import WebPanelSelection from './App/WebPanelSelection';
import WebPanel from './App/WebPanel';

ReactDOM.render(
    <React.Fragment>
        <StyledEngineProvider injectFirst>
            <ThemeProvider theme={darkTheme} >
                <BrowserRouter>
                    <Route exact path='/' render={() => <WebPanelSelection/>}/>
                    <Route exact path='/:planeId/:panelId' render={(props) => <WebPanel planeId={props.match.params.planeId} panelId={props.match.params.panelId} /> } /> 
                </BrowserRouter>
            </ThemeProvider>
        </StyledEngineProvider>
    </React.Fragment>,
    document.getElementById('root')
);