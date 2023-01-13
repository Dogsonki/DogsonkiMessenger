function SearchPageOptionSelector(selectedOption) {
    const AllOptions = ['search-option-all', 'search-option-users', 'search-option-groups'];

    AllOptions.forEach((name) => {
        if (name !== selectedOption) {
            const element = document.getElementById(name);
            if (element.classList.contains('search-option-selected')) {
                element.classList.remove('search-option-selected');
            }
        }
        else {
            document.getElementById(name).classList.add('search-option-selected');
        }
    })
}