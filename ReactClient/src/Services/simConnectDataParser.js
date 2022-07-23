import { API_URL } from "./ServicesConst";

export const initializeDataContainer = async () => {
    let response = await fetch(`${API_URL.url}/getdata`);
    return response.json
}

export const parseRequestData = (resultData) => {
    if (resultData === []) return [];

    let newData = [];

    // Format value as specified by the data key as needed and apply defaults
    resultData.forEach(item => {
        if (item.javaScriptFormatting !== null) {
            item.value = formattingMethod[item.javaScriptFormatting](item.value);
        }

        if (item.value === null || item.value === undefined) {
            item.value = item.defaultValue;
        }

        newData[item.propName] = item.value;
    })

    return newData;
}

export const formattingMethod = {
    toFixed0: (value) => {
        return value.toFixed(0);
    },
    toFixed1: (value) => {
        return value.toFixed(1);
    },
    toFixed2: (value) => {
        return value.toFixed(2);
    },
    toFixed3: (value) => {
        return value.toFixed(3);
    },
    toFixed4: (value) => {
        return value.toFixed(4);
    },
    padStartZero4: (value) => {
        return String(value).padStart(4, '0');
    },
    decToHex: (value) => {
        let str = value.toString(16);
        return str.substring(0, str.length - 4).padStart(4, '0');
    },
    toBlankIfNegative: (value) => {
        if(value < 0)
            return ''
        return value;
    },
    toBlankIfZeroOrNegative: (value) => {
        if(value <= 0)
            return ''
        
        return value;
    },
    toBoeingFlapsValue: (value) => {
        value = value.toFixed(0)

        switch (value) {
            case '0':
                return '0';
            case '1':
                return '1'
            case '2':
                return '2'
            case '3':
                return '5'
            case '4':
                return '10'
            case '5':
                return '15'
            case '6':
                return '25'
            case '7':
                return '30'
            case '8':
                return '40'
            default:
                return '5'
        }
    },
    toBoeingElevatorTrimValue: (value) => {
        return (value / 10).toFixed(2);
    }
}