const postType = {
    CREATE : "create",
    UPDATE : "update",
    DELETE : "delete",
};

const objectType = {
    AREA : "area",
    TABLE : "table"
};

const TableUrls = {
    create: '/Table/CreateTable',
    update: '/Table/UpdateTable',
    delete: '/Table/DeleteTable'
};
const AreaUrls = {
    create: '/Table/CreateArea',
    update: '/Table/UpdateArea',
    delete: '/Table/DeleteArea'
};

const TableDashboardForm = document.getElementById("TableDashboardForm");
const Overlay = document.getElementById("Overlay");

function Load(element){
    element.addEventListener("click", StopEvent, false);
}

function StopEvent(ev) {
    ev.stopPropagation();
}

async function OpenForm (method, type, parentId) {
    await $.ajax({
        url: (type === objectType.TABLE ? TableUrls[method] : AreaUrls[method]),
        data: {
            "guid": parentId
        },
        type: "text/html",
        method: "get",
        success: function (data) {
            document.getElementById("TableDashboardFormFieldset").innerHTML = data;
            TableDashboardForm.action = (type === objectType.TABLE ? TableUrls[method] : AreaUrls[method]) + `?guid=${parentId}`;
            TableDashboardForm.classList.remove("hidden");
            Overlay.classList.remove("hidden");
        },
        error: function (data) {
            console.log(data)
        }
    });
}

function CloseForm() {
    TableDashboardForm.classList.add("hidden");
    Overlay.classList.add("hidden");
}

function ToggleCollapse(me, id) {
    let myCollapse = document.getElementById(id);
    let is = me.getElementsByTagName("i");
    is[is.length - 1].classList.toggle("bi-chevron-up");
    is[is.length - 1].classList.toggle("bi-chevron-down");

    let bsCollapse = new bootstrap.Collapse(myCollapse);
    bsCollapse.toggle();
}