const MenuItemPanel = document.getElementById("MenuItemPanel");
const MenuItemOptionPanel = document.getElementById("MenuItemOptionsPanel");
const CartPanel = document.getElementById("CartPanel");
const SidePanel = document.getElementById("SidePanel");
const BillPanel = document.getElementById("BillPanel");
const Overlay = document.getElementById("Overlay");

async function OpenMenuItemPanel() {
    await $.ajax({
        url: "/Menu/GetMenuItems",
        method: "GET",
        success: function (data) {
            MenuItemPanel.innerHTML = data;
            MenuItemPanel.classList.remove("hidden");
            Overlay.classList.remove("hidden");
        },
        error: function (data) {
            console.Error(data);
        }
    });
}

function CloseMenuItemPanel() {
    MenuItemPanel.classList.add("hidden");
    Overlay.classList.add("hidden");
}

function OpenMenuItemOptionPanel(Id) {
    $.ajax({
        url: `/Orders/GetMenuItemOptions/${Id}`,
        method: 'GET',
        success: function (data) {
            CloseMenuItemPanel();
            MenuItemOptionPanel.innerHTML = data;
            MenuItemOptionPanel.classList.remove("hidden");
            Overlay.classList.remove("hidden");
        },
        error: function (data) {
            console.log(data)
        }
    });
}

async function CloseMenuItemOptionPanel() {
    MenuItemOptionPanel.classList.add("hidden");
    await OpenMenuItemPanel();
}

function AddMenuItem(obj, names) {
    obj["Values"] = {};
    
    if (Array.isArray(names)) {
        names.forEach(function (key) {
            let inputs = document.getElementsByName(key);
            for (let i = 0; i < inputs.length; i++) {
                let input = inputs[i];
                if (!obj["Values"].hasOwnProperty(key)) obj["Values"][key] = [];

                if (input.checked) {
                    obj["Values"][key].push(input.value);
                }
            }
        });
    }
    
    CloseEveryForm();
    CreateMenuItemEntry(obj);
}

function CreateMenuItemEntry(obj) {
    let entry = document.createElement("div");
    entry.classList.add("MenuItemEntry")
    
    let img = document.createElement("img");
    img.src = `/${obj["MenuItem"]["ImagePath"]}`;
    entry.appendChild(img);

    let name = document.createElement("div");
    name.innerText = obj["MenuItem"]["Name"];
    entry.appendChild(name);

    let remove = document.createElement("div");
    remove.classList.add("XButton");
    remove.onclick = function (){
        entry.remove();
    };
    entry.appendChild(remove);

    let hidden = document.createElement("input");
    hidden.type = "hidden";
    hidden.name = hidden.id = "MenuItemOrderInputFormStrings";
    hidden.value = JSON.stringify(obj);
    entry.appendChild(hidden);

    document.getElementById("MenuItemDiv").appendChild(entry);
}

async function ToggleSidePanel(TableId){
    if(SidePanel.classList.contains("hidden"))
        await OpenSidePanel(TableId);
    else
        CloseSidePanel();
}

async function OpenSidePanel(TableId) {
    await $.ajax({
        url: `/Orders/GetTableCart/${TableId}`,
        method: 'GET',
        success: function (data) {
            SidePanel.innerHTML = data;
            SidePanel.classList.remove("hidden");
        },
        error: function () {
            alert("Ops! Something went wrong, Unable to retrieve information. Try again in a few. If error persists contact system administrator.")
        }
    });
}

function CloseSidePanel() {
    SidePanel.classList.add("hidden");
}

async function OpenCartForm(TableId) {
    await $.ajax({
        url: `/Orders/CreateCartPartial/${TableId}`,
        method: "GET",
        response: "html",
        success: function (data) {
            CartPanel.innerHTML = data;
            CloseSidePanel();
            CartPanel.classList.remove("hidden");
            Overlay.classList.remove("hidden");
        },
        error: function () {
            console.log("Error");
        }
    });
}

function CloseCartForm() {
    CartPanel.classList.add("hidden");
}

async function GenerateBill(CartId){
    await $.ajax({
        url: `/Orders/GenerateBill/${CartId}`,
        method: "GET",
        success: function (data) {
            BillPanel.innerHTML = data;
            BillPanel.classList.remove("hidden");
            Overlay.classList.remove("hidden");
        },
        error: function () {
            console.log("Error.")
        }
    });
}

async function CloseBill() {
    BillPanel.classList.add("hidden");
    Overlay.classList.add("hidden");
}

async function OpenMenuItemFromSidePanel(CartId) {
    await $.ajax({
        url: "/Menu/GetMenuItemsFromSidePanel",
        method: "GET",
        data: {
          cartId: CartId
        },
        success: function (data) {
            MenuItemPanel.innerHTML = data;
            MenuItemPanel.classList.remove("hidden");
            Overlay.classList.remove("hidden");
        },
        error: function () {
            console.log("Yello");
        }
    });
}

function OpenMenuItemOptionPanelFromSidePanel(Id, CartId) {
    $.ajax({
        url: `/Orders/GetMenuItemOptionsFromSidePanel/${Id}`,
        method: 'GET',
        data: {
            cartId: CartId
        },
        success: function (data) {
            CloseMenuItemPanel();
            MenuItemOptionPanel.innerHTML = data;
            MenuItemOptionPanel.classList.remove("hidden");
            Overlay.classList.remove("hidden");
        },
        error: function (data) {
            console.log(data)
        }
    });
}

async function AddMenuItemFromSidePanel(obj, names, CartId) {
    obj["Values"] = {};

    if (Array.isArray(names)) {
        names.forEach(function (key) {
            let inputs = document.getElementsByName(key);
            for (let i = 0; i < inputs.length; i++) {
                let input = inputs[i];
                if (!obj["Values"].hasOwnProperty(key)) obj["Values"][key] = [];

                if (input.checked) {
                    console.log(input.value)
                    obj["Values"][key].push(input.value);
                }
            }
        });
    }
    
    await $.ajax({
        url: "/Orders/WaitstaffAddMenuItemToOrder",
        method: "POST",
        data: {
            obj: JSON.stringify(obj),
            cartId: CartId
        },
        success: async function () {
            await CloseMenuItemOptionPanel();
            CreateMenuItemEntry(obj);
        },
        error: function () {
            alert("There was an error sending the information.");
        }
    });
    
}

function CreateMenuItemEntryFromSidePanel(obj) {
    let row = document.createElement("div");
    row.classList.add("MenuItem");

    let col1 = document.createElement("div");
    col1.id = "MenuItemImage";
    let img = document.createElement("img");
    img.src = `/${obj["MenuItem"]["ImagePath"]}`;
    img.width = 64;
    img.height = 64;
    col1.appendChild(img);
    row.appendChild(col1);

    let col2 = document.createElement("div");
    col2.id = "MenuItemName";
    col2.innerHTML = `<p>${obj["MenuItem"]["Name"]}</p>`;
    row.appendChild(col2);

    document.getElementById("TablePreviewMenuItemDiv").appendChild(row);
}

function CloseEveryForm() {
    Overlay.classList.add("hidden");
    MenuItemPanel.classList.add("hidden");
    MenuItemOptionPanel.classList.add("hidden");
}

function LoadBellIcon(){
    Array.from(document.getElementsByClassName("TableButton")).forEach(value => {
        $.ajax({
            url: `/Orders/IsOrderReady/${value.getAttribute('value')}`,
            success: function (data) {
                value.innerHTML = data;
            },
            error: function (data) {
                console.error(data);
            }
        })
    })
}

window.addEventListener("load", LoadBellIcon);
setInterval(LoadBellIcon, 15000);