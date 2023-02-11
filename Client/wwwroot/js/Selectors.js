function SearchPageOptionSelector(selectedOption) {
    const AllOptions = ['search-option-all', 'search-option-users', 'search-option-groups'];
    const selectorClass = 'search-option-selected';

    Selector(AllOptions, selectedOption, selectorClass)
}

function MainPageContextSelector(selectedContext)
{
    const AllContexts = ['last-chats', 'requests'];
    const selectorClass = 'mainpage-context-selected';
    Selector(AllContexts, selectedContext, selectorClass)
}

function Selector(arr, dist, selectorClass) {
    arr.forEach((name) => {
        if (name !== dist) {
            const element = document.getElementById(name);
            if (element.classList.contains(selectorClass)) {
                element.classList.remove(selectorClass);
            }
        }
        else {
            document.getElementById(name).classList.add(selectorClass);
        }
    })
}