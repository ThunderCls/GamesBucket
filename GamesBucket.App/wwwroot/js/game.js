// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

async function applyCatalogFilters() {

    let filterKeyword = document.querySelector("input[name='keywords']").value;
    let sortReleaseType = parseInt(document.querySelector("select[name='sort-release-time']").value);
    let sortBeatType = parseInt(document.querySelector("select[name='sort-beat-time']").value);
    let container = document.querySelector('#result-container');
    let slider = document.getElementById('filter__range');
    let timeRange = slider.noUiSlider.get().map(e => parseInt(e));

    let filterGenresChecks = document.querySelectorAll('#collapseFilter > div > div > ul.filter__checkboxes > li > input[type="checkbox"]');
    let filterGenres;
    if(filterGenresChecks !== null) {
        filterGenres = Array.from(filterGenresChecks)
            .filter(c => c.checked)
            .map(c => c.dataset.value.toLowerCase());
    }
    
    container.querySelectorAll('div.game-card').forEach((item) => {
        if (item != null) {
            let title = item.querySelector('div.card__title > h3 > a');
            if (!title.innerHTML.toLowerCase().includes(filterKeyword.toLowerCase())){
                item.style.display = 'none';
                return;
            }
            
            let beatTime = parseFloat(item.dataset.beatTime);
            if(beatTime > 0 && beatTime < timeRange[0] || beatTime > timeRange[1]){
                item.style.display = 'none';
                return;
            }
            
            let genres = item.dataset.genre.split(';');
            if(filterGenres.length > 0 && genres.every(g => !filterGenres.includes(g))){                    
                item.style.display = 'none';
                return;
            }
            
            item.style.display = 'block';
        }
    });

    let cards = container.querySelectorAll("div.game-card");
    let pagination = document.querySelector('#result-container > div:last-child');
    
    if (sortReleaseType > 0) {        
        await sortCardsByDate(cards, sortReleaseType, container, pagination);
        return;
    }

    if (sortBeatType > 0) {
        await sortCardsByBeatTime(cards, sortBeatType, container, pagination);
    }
}

async function applyFilter() {

    let filterKeyword = document.querySelector("input[name='keywords']").value;
    let filterGenres = document.querySelector("select[name='genres']");
    let sortType = parseInt(document.querySelector("select[name='sort']").value);
    let container = document.querySelector('#result-container');

    container.querySelectorAll('div.game-card').forEach((item) => {
        if (item != null) {
            let title = item.querySelector('div.card__title > h3 > a');
            let genres = item.dataset.genre.split(';');
            let filterGenreText = filterGenres.querySelector(`option[value='${filterGenres.value}']`).innerText

            if (title.innerHTML.toLowerCase().includes(filterKeyword.toLowerCase())) {
                if (filterGenreText.toLowerCase() === 'all genres'
                    || genres.some(g => g.toLowerCase() === filterGenreText.toLowerCase())) {
                    item.style.display = 'block';
                } else {
                    item.style.display = 'none';
                }
            } else {
                item.style.display = 'none';
            }
        }
    });

    if (sortType >= 0) {
        let cards = container.querySelectorAll("div.game-card");
        let pagination = document.querySelector('#result-container > div:last-child');
        await sortCardsByDate(cards, sortType, container, pagination);
    }
}

async function sortCardsByBeatTime(cards, sortBeatType, container, pagination){
    let sortedCards = Array.from(cards).sort((a, b) => {
        let timeA = parseFloat(a.dataset.beatTime);
        let timeB = parseFloat(b.dataset.beatTime);
        switch (sortBeatType){
            case 1: return timeA - timeB;
            case 2: return timeB - timeA;
            default: return 0;
        }
    });

    setContainerCards(container, sortedCards, pagination);
}

async function sortCardsByDate(cards, sortType, container, pagination){
    let sortedCards = Array.from(cards).sort((a, b) => {
        let dateA = new Date(parseInt(a.dataset.releaseDate));
        let dateB = new Date(parseInt(b.dataset.releaseDate));
        switch (sortType){
            case 1: return dateB - dateA;
            case 2: return dateA - dateB;
            default: return 0;
        }
    });

    setContainerCards(container, sortedCards, pagination);
}

function setContainerCards(container, sortedCards, pagination){
    container.innerHTML = '';
    sortedCards.forEach((card) => {
        container.append(card);
    });
    container.append(pagination);
}

async function editFromCatalog(btnAction, steamAppId) {
    let url = location.origin;
    $.confirm({
        title: 'Edit Details!',
        theme: 'dark',
        type: 'purple',
        content: `url:${url}/catalog/editgame?appId=${steamAppId}`,
        buttons: {
            formSubmit: {
                text: 'Save',
                btnClass: 'btn-dlg-confirm',
                action: function () {
                    let form = document.querySelector('.edit-game-form');
                    if(form === null || !form.checkValidity()){
                        $.alert({
                            content: "Please add a game name before submitting",
                            type: 'purple',
                            theme: 'dark',
                        });
                        return false;
                    } else {
                        form.submit();
                    }
                }
            },
            cancel: {
                btnClass: 'btn-dlg-cancel',
                action: function () {
                }
            }
        },
        onContentReady: function () {
            // bind to events
            let jc = this;
            this.$content.find('form').on('submit', function (e) {
                // if the user submits the form by pressing enter in the field.
                e.preventDefault();
                jc.$$formSubmit.trigger('click'); // reference the button and click it
            });
        }
    });
}

async function editCatalog(btnAction, action, steamAppId) {
    if (action === 'save') {
        await saveToCatalog(btnAction, steamAppId);
    } else if (action === 'remove') {
        await removeFromCatalog(btnAction, steamAppId);
    } else if(action === 'edit'){
        await editFromCatalog(btnAction, steamAppId);
    }
}

async function saveToCatalog(btnAction, steamAppId){
    await toggleLoader();
    //let url = window.location.protocol + "//" + location.host.split(":")[0];
    let url = location.origin;
    const gameId = { appId: steamAppId };
    const bodyData = toUrlEncoded(gameId);    
    try {
        let response = await fetch(`${url}/catalog/save`, {
            method : 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: bodyData
        });

        await toggleLoader();
        
        if(response.status === 201){
            // switch button
            if(btnAction !== null){
                btnAction.dataset.action = 'remove';
                btnAction.innerHTML = 'Remove from Catalog';
                btnAction.className = 'btn-delete-static-wide btn-delete';                
            }

            // show fav button
            let btnFav = document.querySelectorAll('.details__favorite');
            if(btnFav !== null){
                Array.from(btnFav).forEach((item) => {
                    item.style.display = 'block';
                });
            }
            
            showToastr("Success!", "Game added successfully to your catalog", "success");
            return;
        } else {
            showToastr("Warning!", "Something weird happened!", "warning");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Error!", "An error occured while adding the game", "error");
}

async function removeGameCard(steamAppId) {
    let container = document.querySelector('#result-container');
    if(container === null) return;
    
    let cards = container.querySelectorAll("div.game-card");
    let pagination = document.querySelector('.paginator');
    if(pagination !== null) pagination = pagination.parentElement;
    
    let catalogCards = Array.from(cards).filter(card => {
        let element = card.querySelector("div > div.card__actions > button");
        let steamId = element.dataset.steamid;
        return (steamId !== steamAppId);
    });
    container.innerHTML = '';
    if(catalogCards.length === 0){
        let parentContainer = document.querySelector("body > section.section.section--last.section--catalog > div");
        parentContainer.innerHTML = '<div class="row mt-5">\n' +
            '\t\t\t\t<div class="col-md-12 mt-5">\n' +
            '\t\t\t\t\t<i class="fa fa-search not-found-icon" aria-hidden="true"></i>\n' +
            '\t\t\t\t\t<span class="not-found-text">Empty Catalog</span>\n' +
            '\t\t\t\t</div>\n' +
            '\t\t\t</div>';
    } else {
        catalogCards.forEach((card) => {
            container.append(card);
        });
        container.append(pagination);        
    }    
}

async function removeFromCatalog(btnAction, steamAppId){
    $.confirm({
        title: 'Remove Game!',
        content: 'Remove this game from your catalog?',
        theme: 'dark',
        type: 'purple',
        closeIcon: false,
        buttons: {
            ok: {
                btnClass: 'btn-dlg-confirm',
                action: async function () {
                    await toggleLoader();
                    //let url = window.location.protocol + "//" + location.host.split(":")[0];
                    let url = location.origin;
                    const gameId = {appId: steamAppId};
                    const bodyData = toUrlEncoded(gameId);
                    try {
                        let response = await fetch(`${url}/catalog/remove`, {
                            method : 'post',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                            },
                            body: bodyData
                        });

                        await toggleLoader();

                        if(response.status === 200){
                            
                            // if it was a custom game then go back to catalog
                            if(steamAppId.length > 30)
                                window.location = `${url}/catalog`;
                            
                            // switch button
                            if(btnAction !== null){
                                btnAction.dataset.action = 'save';
                                btnAction.innerHTML = 'Add to Catalog';
                                btnAction.className = 'details__buy';                                
                            }

                            // hide fav button
                            let btnFav = document.querySelectorAll('.details__favorite');
                            if(btnFav !== null){
                                Array.from(btnFav).forEach((item) => {
                                    item.style.display = 'none';
                                });
                            }
                            
                            // remove game card
                            await removeGameCard(steamAppId);
                            
                            showToastr("Success!", "Game removed successfully from your catalog", "success");
                            return;
                        } else {
                            showToastr("Warning!", "Something weird happened!", "warning");
                            return;   
                        }                            
                    } catch (e) {
                        console.log(e);
                    }

                    await toggleLoader();
                    showToastr("Error!", "An error occured while removing the game", "error");
                }
            },
            cancel: {
                btnClass: 'btn-dlg-cancel'                
            }            
        }
    });
}

const toUrlEncoded = obj => 
    Object.keys(obj).map(k => encodeURIComponent(k) + '=' + encodeURIComponent(obj[k])).join('&');

/**
 * Show a toastr alert
 * @param {any} title
 * @param {any} text
 * @param {any} type
 */
function showToastr(title, text, type) {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": false,
        "positionClass": "toast-top-full-width",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "2000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

    toastr[type](text, title);
}

async function toggleLoader(){
    let loader = document.querySelector('.spinner-border-body');
    if(loader === null) return;
    
    if(loader.style.visibility !== 'visible'){
        loader.style.visibility = 'visible';
    } else {
        loader.style.visibility = 'hidden';
    }
}