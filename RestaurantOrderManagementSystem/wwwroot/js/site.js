$(document).ready( function() {
    $(".sidebar-dropdown-menu").on("mouseleave", function () {
        if ($("#Sidebar").hasClass("shrunk"))
            $(".sidebar-dropdown-menu").removeClass("show");
    });
});

const CloseOnOutsideClickElements = Array.from(document.getElementsByClassName("CloseOnOutsideClick"));

document.addEventListener('click', function(event) {
    if (event.target.classList.contains("CloseOnOutsideClick")) {
        event.stopPropagation();
    } else {
        CloseOnOutsideClickElements.forEach(value => value.classList.remove("show"));
    }
});