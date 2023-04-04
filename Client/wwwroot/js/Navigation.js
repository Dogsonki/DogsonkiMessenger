function NavigationPreventBack() {
    window.addEventListener("popstate", function (e) {
    }, false)
}

function NavigationGoBack() {
    history.go(-1);
}