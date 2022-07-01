import React, { createContext, useContext, useState, useEffect } from 'react';
import { API_URL } from "./ServicesConst";

const TOUCH_PANEL_SETTINGS_REQUEST_INTERVAL = 2000;
const TOUCH_PANEL_SETTINGS_REQUEST_INTERVAL_SLOW = 5000;

const LocalStorageDataContext = createContext(null);

const LocalStorageProvider = ({ initialData, children }) => {
    const [configurationData, setConfigurationData] = useState(initialData);
    const [mapConfig, setMapConfig] = useState();

    useEffect(() => {
        // Set default map config
        setMapConfig({
            flightFollowing: true,
            showFlightPlan: true,
            showFlightPlanLabel: false,
            currentLayer: 'Bing Roads'
        });       

        let requestInterval = null;

        const requestTouchPanelSettings = async () => {
            let response = await fetch(`${API_URL.url}/gettouchpanelsettings`).catch(() => {
                console.error('Unable to get touch panel settings from MSFS Touch Panel Server.');
                clearInterval(requestInterval);
                requestInterval = setInterval(() => requestTouchPanelSettings(), TOUCH_PANEL_SETTINGS_REQUEST_INTERVAL_SLOW); 
                return;
            });

            if(response !== undefined)
            {
                let data = await response.json();

                if(data !== undefined)
                {
                    updateConfigurationData(data);

                    clearInterval(requestInterval);
                    requestInterval = setInterval(() => requestTouchPanelSettings(), TOUCH_PANEL_SETTINGS_REQUEST_INTERVAL);     // Ping server for updates of touch panel settings every 2 seconds
                }
            }
        }

        requestTouchPanelSettings();
    }, [])

    const initializeLocalStorage = () => {
        // Default settings
        let settings = {
            dataRefreshInterval: 100,
            mapRefreshInterval: 250,
            isUsedArduino: false,
            isEnabledSound: true
        };

        localStorage.setItem('settings', JSON.stringify(settings));
    }

    const updateConfigurationData = (value) => {
        localStorage.setItem('settings', JSON.stringify(value));
        setConfigurationData(value);
    }

    // get data from local storage
    useEffect(() => {
        if (localStorage.getItem('settings') === null)
            initializeLocalStorage();

        setConfigurationData(JSON.parse(localStorage.getItem('settings')));
    }, [])

    return (
        <LocalStorageDataContext.Provider value={{ configurationData, mapConfig }}>
            {children}
        </LocalStorageDataContext.Provider>
    )
}

export default LocalStorageProvider;

// custom hook
export const useLocalStorageData = () => useContext(LocalStorageDataContext);