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
        let localSettings = localStorage.getItem('settings');
        let updateInterval = localSettings !== null ? JSON.parse(localSettings).dataRefreshInterval : 500;
        
        // Using sharedworker
        // sharedWorker = new SharedWorker('/sharedworker.js');
        // sharedWorker.port.start();  
        // sharedWorker.port.postMessage({apiUrl: API_URL.url, updateInterval: updateInterval});
        // sharedWorker.port.onmessage = e => {  
        //     if(e.data !== null && e.data !== undefined)
        //         setArduinoStatus(Boolean(e.data.arduinoStatus));
        //         setNetworkStatus(Boolean(e.data.networkStatus));

        //         if (e.data.systemEvent !== undefined && e.data.systemEvent !== null)
        //             setSimConnectSystemEvent(e.data.systemEvent.split('-')[0]);
        //         else
        //             setSimConnectSystemEvent(null);

        //         if(e.data.simConnectData !== undefined && e.data.simConnectData !== null)
        //             setSimConnectData(e.data.simConnectData);
        // };
    
        const requestData = async () => {
            try {
                let response = await fetch(`${API_URL.url}/getdata`).catch(() => {
                    throw('MSFS Touch Panel Server is not available.')
                });

                if (response !== undefined) {
                    let result = await response.json();

                    if (result === undefined)
                        throw new Error('MSFS Touch Panel Server error');

                    setNetworkStatus(Boolean(result.msfsStatus ?? false));
                    setArduinoStatus(Boolean(result.arduinoStatus ?? false));
                    
                    if (!result.msfsStatus)
                        throw('MSFS SimConnect is not available.')

                    if (result.systemEvent !== null && result.systemEvent !== undefined)
                        setSimConnectSystemEvent(result.systemEvent.split('-')[0]);
                    else
                        setSimConnectSystemEvent(null);

                    var simData = JSON.parse(result.data ?? []);
                        
                    if ((simData !== null && simData !== []))
                        setSimConnectData(parseRequestData(simData));
                    
                    
                    setTimeout(() => requestData(), updateInterval);
                }
                else {
                    throw('Empty MSFS Touch Panel Server response.')
                }
            }
            catch (error) {
                console.log(error);
                setNetworkStatus(false);
                setTimeout(() => requestData(), SIMCONNECT_DATA_REQUEST_INTERVAL_SLOW);
            }
        }

        requestData();
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

export const simConnectGetPlanePanelProfilesInfo = async () =>
 {
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