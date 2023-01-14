function ScrollToBottom(element) {

    if (element === null) {
        throw 'Parameter "element" is null';
    }

    const messageBoxLastElement = element.lastElementChild;

    const messageFrameLastElement = messageBoxLastElement.getElementsByClassName("message-body-frame")[0].lastElementChild;

    //Scroll to last element of body frame
    messageFrameLastElement.scrollIntoView(true);

    const offsetY = element.getBoundingClientRect().top + window.pageYOffset + -50;

    //Scroll by offset
    window.scrollTo({ top: offsetY, behavior: 'smooth' });
}

function ScrollToBottomByDiv(divElement) {
    divElement.scrollTop = divElement.scrollHeight;
}