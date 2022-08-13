import React, { createContext, useContext, useState, useEffect } from 'react';
import { API_URL } from "./ServicesConst";
import { initializeDataContainer, parseRequestData } from './simConnectDataParser';
import jref from 'json-ref-lite'

const SIMCONNECT_DATA_REQUEST_INTERVAL_SLOW = 5000;
const SimConnectDataContext = createContext(null);

const SimConnectDataProvider = ({ children }) => {
    const [simConnectData, setSimConnectData] = useState(initializeDataContainer());
    const [networkStatus, setNetworkStatus] = useState(null);
    const [arduinoStatus, setArduinoStatus] = useState(null);
    const [simConnectSystemEvent, setSimConnectSystemEvent] = useState(null);

    // request data from SimConnect on timer interval
    useEffect(() => {
        let requestInterval = null;

        let localSettings = localStorage.getItem('settings');

        if (localSettings !== null)
            requestInterval = JSON.parse(localStorage.getItem('settings')).dataRefreshInterval;
        else
            requestInterval = 500;

        const requestData = async () => {
            try {
                let response = await fetch(`${API_URL.url}/getdata`).catch(() => {
                    handleConnectionError('MSFS Touch Panel Server is not available.')
                    return;
                });

                if (response !== undefined) {
                    let result = await response.json();

                    if (result === undefined)
                        throw new Error('MSFS Touch Panel Server error');

                    if (result.msfsStatus !== null && result.msfsStatus !== undefined)
                        setNetworkStatus(Boolean(result.msfsStatus));
                    else
                        setNetworkStatus(false);

                    if (result.arduinoStatus !== null && result.arduinoStatus !== undefined)
                        setArduinoStatus(Boolean(result.arduinoStatus));
                    else
                        setArduinoStatus(false);

                    if (result.systemEvent !== null && result.systemEvent !== undefined)
                        setSimConnectSystemEvent(result.systemEvent.split('-')[0]);
                    else
                        setSimConnectSystemEvent(null);

                    if (!result.msfsStatus)
                    {
                        handleConnectionError('MSFS SimConnect is not available.')
                        return;
                    }

                    if (result.data !== null && result.data !== undefined) {
                        var simData = JSON.parse(result.data);

                        if ((simData !== null && simData !== [])) {
                            setSimConnectData(parseRequestData(simData));
                            clearInterval(requestInterval);
                            let updateInterval = JSON.parse(localStorage.getItem('settings')).dataRefreshInterval;
                            requestInterval = setInterval(() => requestData(), updateInterval);
                        }
                    }
                    else {
                        clearInterval(requestInterval);
                        let updateInterval = JSON.parse(localStorage.getItem('settings')).dataRefreshInterval;
                        requestInterval = setInterval(() => requestData(), updateInterval);
                    }
                }
            }
            catch (error) {
                console.log(error);
                setNetworkStatus(false);
                handleConnectionError('MSFS Touch Panel Server is not available.')
            }
        }

        const handleConnectionError = (errorMessage) => {
            console.error(errorMessage);
            clearInterval(requestInterval);
            requestInterval = setInterval(() => requestData(), SIMCONNECT_DATA_REQUEST_INTERVAL_SLOW);      // slow down the request data interval until network reconnection
        }

        requestData();

        return () => {
            clearInterval(requestInterval);
        }
    }, [])

    return (
        <SimConnectDataContext.Provider value={{ simConnectData, networkStatus, arduinoStatus, simConnectSystemEvent }}>
            {children}
        </SimConnectDataContext.Provider>
    )
}

export default SimConnectDataProvider;

// custom hook
export const useSimConnectData = () => useContext(SimConnectDataContext);

export const simConnectGetPlanePanelProfilesInfo = async () => {
    try {
        let response = await fetch(`${API_URL.url}/getplanepanelprofileinfo`);
        let data = await response.json();

        for (const plane of data) {
            for (const panel of plane.panels) {
                await Promise.all(panel.subPanels.map(async subPanel => {
                    let result = await getLocalPopoutPanelDefinitions(panel.rootPath, subPanel.rootPath);
                    var panelData = result.find(x => x.id === subPanel.definitions);

                    if (panelData !== null) {
                        subPanel.panelSize = panelData.value.panelSize;
                        subPanel.backgroundImage = panelData.value.backgroundImage;
                        subPanel.definitions = panelData.value.panelControlDefinitions;
                        subPanel.parentRootPath = panel.rootPath;
                        subPanel.parentPanelSize = panel.panelSize;
                    }
                }))
            }
        }

        return data;
    }
    catch {
        console.error('Unable to retrieve plane panel profiles. There may be an error with PlanePanelProfile.json.');
        return null;
    }
}

export const getLocalPopoutPanelDefinitions = async (panelRootPath, subPanelRootPath) => {
    try {
        let response = await fetch(`/config/profiles/${panelRootPath}/${subPanelRootPath}/PopoutPanelDefinition.json`, { headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' } })

        let data = await response.json();

        if (!Array.isArray(data))
            data = [data];

        // resolve ref pointers within JSON
        for(let i = 0; i < data.length; i++)
        {
            data[i] = jref.resolve(data[i]);
        }
        return data;
    }
    catch {
        console.error('Unable to retrieve pop out panel definitions. There may be an error with PopoutPanelDefinition.json.')
        return null;
    }
}

export const simConnectGetFlightPlan = async () => {
    try {
        let response = await fetch(`${API_URL.url}/getflightplan`);
        let result = await response.json();

        return result;
    }
    catch {
        console.error('MSFS unable to load flight plan.')
    }
}