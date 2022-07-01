import { API_URL } from "./ServicesConst";

export const simConnectPost = (postData) => {
    if(postData.action === undefined)
        return;

    // default actionValue to 1 if not specified
    if(postData.actionValue === undefined)
        postData.actionValue = 1;

    fetch(`${API_URL.url}/postdata`, {
         method: "POST",
         headers: { "Content-type": "application/json; charset=UTF-8" },
         body: JSON.stringify(postData)
    })
}