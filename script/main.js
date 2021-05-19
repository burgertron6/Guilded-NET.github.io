// Map of all icons and in which classes it should be in
const icons = {
    'info-block': 'fas fa-info-circle',
    'warning-block': 'fas fa-exclamation-circle',
    'caution-block': 'fas fa-times-circle',
    'success-block': 'fas fa-check-circle',
    'note-block': 'fas fa-chevron-circle-right'
};
// Gets all icon variable properties
const iconProps = Object.getOwnPropertyNames(icons)
/**
 * Adds icon on the elements.
 */
function addBlockIcons() {
    // Gets all blockquote elements
    const elems = document.querySelectorAll(`blockquote`)
    // 
    for(let elem of elems) {
        // Gets a first icon which is included in the element's class list
        const iconProp = iconProps.find(x => elem.classList.contains(x))
        // Generates element for blockquote icon
        const iconElement = iconProp != undefined ? `<div class="quote-side"><i class="${icons[iconProp]}"></i></div>` : ``
        // Adds that icon to the element
        elem.innerHTML = `${iconElement} <div class="quote-body">${elem.innerHTML.trim()}</div>`
 
    }
}
document.onreadystatechange = () => {
    if(document.readyState === 'complete')
        addBlockIcons()
}