class PopOutManagerPanelElement extends UIElement {
    constructor() {
        super(...arguments);
        this.ingameUi = null;
        this.isInitialized = false;
        this.panelActive = false;
        this.webSocket = null;
        this.webSocketConnected = false;
        this.webSocketInterval = null;

        this.tryConnectWebSocket = () => {
            this.lockPanel(true);

            this.webSocket = new WebSocket("ws://localhost:27011/ws");

            this.webSocket.onopen = () => {
                clearInterval(this.webSocketInterval);
                this.webSocketConnected = true;
            };

            this.webSocket.onclose = () => {
                this.webSocketConnected = false;

                // Clear panel table
                this.createPanelConfigTableHeader(this.panelConfigTable);
                this.lockPanel(true);

                this.webSocketInterval = setInterval(() => {
                    if (!this.webSocketConnected)
                        this.tryConnectWebSocket();
                }, 2000)
            };

            this.webSocket.onerror = () => {
                this.webSocket.close();
            };

            this.webSocket.onmessage = (event) => {
                if (event.data !== undefined) {

                    var panelData = JSON.parse(event.data);

                    // only recreate panel rows if panel is refreshed (minimize/maximize, pop out)
                    if (this.panelActive && document.getElementsByClassName("panelRow").length == 1)
                        this.createPanelConfigTableBody(this.panelConfigTable, panelData);

                    if (panelData !== undefined && panelData !== null && panelData.length !== 0) {
                        this.lockPanel(false);
                        this.bindPanelData(panelData);
                    }
                }
            };
        }

        this.webSocketInterval = setInterval(() => {
            if (!this.webSocketConnected)
                this.tryConnectWebSocket();
        }, 2000)
    }

    connectedCallback() {
        super.connectedCallback();

        this.ingameUi = this.querySelector('ingame-ui');
        this.panelSelection = document.getElementById("panelSelection");
        this.panelConfiguration = document.getElementById("panelConfiguration");
        this.planeProfileName = document.getElementById("planeProfileName");
        this.panelConfigTable = document.getElementById("panelConfigTable");
        this.btnLockPanels = this.querySelector('#btnLockPanels');
        this.btnPlusTen = this.querySelector('#btnPlusTen');
        this.btnPlusOne = this.querySelector('#btnPlusOne');
        this.btnMinusTen = this.querySelector('#btnMinusTen');
        this.btnMinusOne = this.querySelector('#btnMinusOne');

        this.dropdownProfile = this.querySelector("#dropdownProfile");
        this.addProfile = document.getElementById("addProfile");
        this.deleteProfile = document.getElementById("deleteProfile");

        this.stepBeginDialog = document.getElementById("stepBeginDialog");


        this.closeDialog = document.getElementById("closeDialog");
        
      //this.deleteProfile.disable(!this.deleteProfile.disabled);

        //this.dropdownProfile.addEventListener("select", (event) = {});

        let workflow = new Workflow(this);
        workflow.bindAllButtonEvents();
        workflow.stepBegin();
        

        this.addProfile.addEventListener("click", (event) => {
            addProfileDialog.style.display = "block";
            //this.ingameUi.toggleExternPanel(true);
            document.body.classList.toggle('modal-open');
            
            // let value = new DataValue;
            // value.name = "New Profile " + dropDownValues.length;
            // value.ID = dropDownValues.length;
            // dropDownValues.push(value);
            // this.dropdownProfile.setData(dropDownValues, dropDownValues.length - 1);
        });

        this.deleteProfile.addEventListener("click", (event) => {
            // dropDownValues.splice(-1,1);
            // this.dropdownProfile.setData(dropDownValues, dropDownValues.length - 1);
        });

   
        this.isLocked = false;

        let profiles = ["Kodiak", "172", "737"];
        let dropDownValues = [];
        profiles.forEach((profile, index) => {
            let value = new DataValue;
            value.name = profile;
            value.ID = index;
            dropDownValues.push(value);
        });

        setTimeout(() => {
            this.dropdownProfile.setData(dropDownValues, 2)


        }, 1000);
        


        this.btnLockPanels.addEventListener("click", () => {
            if (this.btnLockPanels.disabled)
                return;

            this.isLocked = !this.isLocked;

            this.lockPanel(this.isLocked);

            if (this.isLocked) {
                this.btnLockPanels.disabled = false;
                this.btnLockPanels.title = "Unlock Panels";
                this.btnLockPanels.style.backgroundColor = "red";
            }
            else {
                this.btnLockPanels.disabled = false;
                this.btnLockPanels.title = "Lock Panels";
                this.btnLockPanels.style.backgroundColor = null;
            }

            this.panelSelection.style.display = 'block';
            this.panelConfiguration.style.display = 'none';
        });


        if (this.ingameUi) {
            this.ingameUi.addEventListener("panelActive", (e) => {
                //document.getElementsByClassName("Extern")[0].style.display = "none";    // disable extern button
                this.createPanelConfigTableHeader(this.panelConfigTable);

                this.panelActive = true;
                this.lockPanel(true);

                if (this.webSocketConnected)
                    this.webSocket.send("RequestPanelData");
            });
        }
    }

    createPanelConfigTableHeader(panelConfigTable) {
        let panelRow;

        // remove all child of panelConfigTable
        while (panelConfigTable.firstChild) {
            panelConfigTable.removeChild(panelConfigTable.firstChild);
        }

        // create header
        panelRow = this.createPanelRow(true);
        panelRow.appendChild(this.createPanelCell("div", "column1", "Panel Name"));
        panelRow.appendChild(this.createPanelCell("div", "column2", "X-Pos"));
        panelRow.appendChild(this.createPanelCell("div", "column3", "Y-Pos"));
        panelRow.appendChild(this.createPanelCell("div", "column4", "Width"));
        panelRow.appendChild(this.createPanelCell("div", "column5", "Height"));
        panelRow.appendChild(this.createPanelCell("div", "column6", "Always on Top"));
        panelRow.appendChild(this.createPanelCell("div", "column7", "Hide Title Bar"));
        panelRow.appendChild(this.createPanelCell("div", "column8", "Full Screen Mode"));
        panelRow.appendChild(this.createPanelCell("div", "column9", "Touch Enabled"));
        panelConfigTable.appendChild(panelRow);
    }

    createPanelConfigTableBody(panelConfigTable, panelData) {
        let panelRow;

        if (panelConfigTable !== undefined) {
            for (let index = 0; index < panelData.length; index++) {
                panelRow = this.createPanelRow(false);
                panelRow.appendChild(this.createPanelCell("div", "column1", this.createUiInput("PanelName_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column2", this.createUiInput("XPos_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column3", this.createUiInput("YPos_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column4", this.createUiInput("Width_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column5", this.createUiInput("Height_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column6", this.createCheckbox("AlwaysOnTop_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column7", this.createCheckbox("HideTitleBar_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column8", this.createCheckbox("FullScreenMode_" + index)));
                panelRow.appendChild(this.createPanelCell("div", "column9", this.createCheckbox("TouchEnabled_" + index)));
                panelConfigTable.appendChild(panelRow);
            }
        }
    }

    createPanelRow(isHeaderRow) {
        let panelRow = document.createElement("div");

        if (isHeaderRow)
            panelRow.classList.add("panelHeaderRow");

        panelRow.classList.add("panelRow");

        return panelRow;
    }

    createPanelCell(cellType, classes = null, childElement) {
        let cell = document.createElement(cellType);
        cell.classList.add("panelCell");

        if (classes !== undefined || classes !== null) {
            if (typeof (classes) === "string") {
                cell.classList.add(classes);
            }
            else {
                for (let index = 0; index < classes.length; index++)
                    cell.classList.add(classes[index]);
            }
        }

        if (typeof (childElement) === "string")
            cell.innerHTML = `<div class='alignCenter'>${childElement}</div>`;
        else
            cell.appendChild(childElement);

        return cell;
    }

    createUiInput(id) {
        let input = document.createElement("ui-input");
        input.type = "text";
        input.id = id;
        return input;
    }

    createCheckbox(id) {
        let checkbox = document.createElement("checkbox-element");
        checkbox.id = id;
        checkbox.style.width = "1em";
        return checkbox;
    }

    lockPanel(isLocked) {
        if (this.panelActive) {
            let uiInputs = document.getElementsByTagName("ui-input");
            for (let index = 0; index < uiInputs.length; index++) {
                uiInputs[index].disabled = isLocked;
            }

            let checkboxes = document.getElementsByTagName("checkbox-element");
            for (let index = 0; index < checkboxes.length; index++) {
                checkboxes[index].SetData({ sTitle: "", bToggled: checkboxes.toggled, bDisabled: isLocked });
                checkboxes[index].RefreshValue();
            }

            this.btnLockPanels.disabled = isLocked;
            if(isLocked)
            {
                this.btnLockPanels.title = "Lock Panels";
                this.btnLockPanels.style.backgroundColor = null;
            }

            this.btnPlusTen.disabled = isLocked;
            this.btnPlusOne.disabled = isLocked;
            this.btnMinusTen.disabled = isLocked;
            this.btnMinusOne.disabled = isLocked;
        }
    }

    bindPanelData(panelData) {
        if (this.panelActive) {
            panelData.forEach((panel, index) => {
                this.bindUiInput("PanelName_" + index, panel.panelName, "text");
                this.bindUiInput("XPos_" + index, panel.xPos);
                this.bindUiInput("YPos_" + index, panel.yPos);
                this.bindUiInput("Width_" + index, panel.width);
                this.bindUiInput("Height_" + index, panel.height);
                this.bindCheckbox("AlwaysOnTop_" + index, panel.alwaysOnTop);
                this.bindCheckbox("HideTitleBar_" + index, panel.hideTitleBar);
                this.bindCheckbox("FullScreenMode_" + index, panel.fullScreenMode);
                this.bindCheckbox("TouchEnabled_" + index, panel.touchEnabled);
            })
        }
    }

    bindUiInput(id, value, type) {
        let input = document.getElementById(id);
        input.style.width = "100%";
        input.setValue(value);

        if (type === "text")
            input.children[0].children[0].style.textAlign = "left";
    }

    bindCheckbox(id, value) {
        let checkbox = document.getElementById(id);
        checkbox.SetData({ sTitle: "", bToggled: value, bDisabled: false });
        checkbox.RefreshValue();
    }
}
window.customElements.define("pop-out-manager", PopOutManagerPanelElement);






class Workflow {
    constructor(owner) {
        this.owner = owner;
    }

    get workflowSteps() {  
        return [
        { 
            name: 'stepBegin',
            results: 
            [
                { value: 'stepCreateNewProfile' },
                { value: 'stepStartPopOut' },
                { value: 'stepAdjustProfile' }
            ]
        }]
    }

    getFuncName() {
        return this.getFuncName.caller.name;
    }

    bindAllButtonEvents(){
        let stepButtons = document.getElementsByClassName("stepButton");
        Array.from(stepButtons).forEach(btn => {
            btn.addEventListener("click", (e) => {
                let parentStep = e.target.getAttribute("parentStep");
                let resultValue = e.target.id.replace("btn", "step");
                this.handleButtonClick(parentStep, resultValue);
            })
        })
    }

    handleButtonClick(currentStep, resultValue) {
        let step = this.workflowSteps.find(c => c.name == currentStep);
        let result = step.results.find(f => f.value == resultValue);

        if(result != null)
        {
            let func = this[result.value];
            func();
        }
    }

    openDialog(step)
    {
        this.owner.stepBeginDialog = document.getElementById(step.replace("step", "dialog"));
        this.owner.stepBeginDialog.style.display = "block";
        document.body.classList.toggle("modal-open");
    }

    stepBegin() {
        this.openDialog("stepBegin");
    }

    stepStartPopOut() {
        var a = ""
    }

    stepCreateNewProfile() {
        var a = ""
    }

    stepAdjustProfile() {
        var a = ""
    }
}