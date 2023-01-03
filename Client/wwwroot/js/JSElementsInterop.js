function ScrollToBottom(element) {

    if (element === null) {
        throw 'Parameter "element" is null';
    }

    const lastChild = element.lastElementChild;

    lastChild.scrollIntoView();
}