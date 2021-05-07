function ToggleCheckbox(element) {
    if (element.checked)
        element.parentElement.classList.add("checked");
    else
        element.parentElement.classList.remove("checked");
}

function ToggleRadios(elements){
    elements.forEach(value => {
        if (value.checked)
            value.parentElement.classList.add("checked");
        else
            value.parentElement.classList.remove("checked");
    })
}

function ToggleDropdown(id) {
    $(`#${id}`).toggleClass("show");
}

Array.from(document.getElementsByClassName("Checkbox-Input")).forEach(value => ToggleCheckbox(value));
ToggleRadios(Array.from(document.getElementsByClassName("Radio-Input")));