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

    let filterGenresChecks = document
        .querySelectorAll('#collapseFilter > div > div > ul.filter__checkboxes > li > input[type="checkbox"]');
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
            
            let genres = item.dataset.genre.split(',');
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
            let genres = item.dataset.genre.split(',');
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
 
function resetCatalogOrderFilters(activeFilter) {
    let filterSelects = document.querySelectorAll('div.catalog-order-by-filters > div > div > select');
    filterSelects.forEach(select => {
        if (select !== null && select !== activeFilter)
            select.value = 0;
    });
}

function clearCatalogGenresFilters() {
    let filterSelects = document.querySelectorAll('ul#genres-filters > li > input');
    filterSelects.forEach(select => {
        select.checked = false;
    });
}

function selectAllCatalogGenresFilters() {
    let filterSelects = document.querySelectorAll('ul#genres-filters > li > input');
    filterSelects.forEach(select => {
        select.checked = true;
    });
}

async function filterGamesHomeSearch(pageNumber = 1){
    await toggleLoader();
    //let url = window.location.protocol + "//" + location.host.split(":")[0];
    let url = location.origin;
    let urlParams = new URLSearchParams(window.location.search);
    let gameTitle = urlParams.get('query');        
    
    // get filter criteria elements
    let pageSize = document.querySelector("select[name='page-size']").value;
    const filterData = {
        page: pageNumber,
        pageSize: pageSize,
        gameTitle: gameTitle
    };

    const bodyData = toUrlEncoded(filterData);
    try {
        let response = await fetch(`${url}/home/searchfilter`, {
            method: 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: bodyData
        });

        await toggleLoader();

        if (response.status === 200) {
            let gameCards = await response.text();
            if(gameCards.length > 0){
                let container = document.querySelector('#result-container');
                container.innerHTML = '';
                container.innerHTML = gameCards;
            }
            return;
        } else {
            let textResult = await response.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}

async function filterGamesSearch(pageNumber = 1){
    await toggleLoader();
    //let url = window.location.protocol + "//" + location.host.split(":")[0];
    let url = location.origin;
    let urlParams = new URLSearchParams(window.location.search);
    let gameTitle = urlParams.get('query');
        
    // get filter criteria elements
    let sortBy = '';
    let sortType = '';
    let releaseTimeFilter = document.querySelector("select[name='sort-release-date']");
    let beatTimeFilter = document.querySelector("select[name='sort-beat-time']");
    let pageSize = document.querySelector("select[name='page-size']").value;

    if(releaseTimeFilter.value > 0){
        sortBy = 'release_date';
        switch (releaseTimeFilter.value){
            case '1': sortType = 'desc'; break;
            case '2': sortType = 'asc'; break;
        }
    }
    if (beatTimeFilter.value > 0){
        sortBy = 'beat_time';
        switch (beatTimeFilter.value){
            case '1': sortType = 'desc'; break;
            case '2': sortType = 'asc'; break;
        }
    }

    const filterData = {
        sortBy: sortBy,
        sortType: sortType,
        page: pageNumber,
        pageSize: pageSize,
        gameTitle: gameTitle
    };

    const bodyData = toUrlEncoded(filterData);
    try {
        let response = await fetch(`${url}/catalog/search/filter`, {
            method: 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: bodyData
        });

        await toggleLoader();

        if (response.status === 200) {
            let gameCards = await response.text();
            if(gameCards.length > 0){
                let container = document.querySelector('#result-container');
                container.innerHTML = '';
                container.innerHTML = gameCards;
            }
            return;
        } else {
            let textResult = await response.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
} 

async function getGamesPage(pageNumber = 1) {
    await toggleLoader();
    //let url = window.location.protocol + "//" + location.host.split(":")[0];
    let url = location.origin;
    
    // get filter criteria elements
    // beat time range
    let slider = document.getElementById('filter__range');
    let timeRange = slider.noUiSlider.get().map(e => parseInt(e));
    // genres
    let filterGenresChecks = document
        .querySelectorAll('#collapseFilter > div > div > ul.filter__checkboxes > li > input[type="checkbox"]');
    let filterGenres;
    if(filterGenresChecks !== null) {
        filterGenres = Array.from(filterGenresChecks)
            .filter(c => c.checked)
            .map(c => c.dataset.value.toLowerCase());    
    }
    // filters
    let sortBy = '';
    let sortType = '';
    let completionStatus = '';
    let favStatus = '';
    let releaseTimeFilter = document.querySelector("select[name='sort-release-time']");
    let beatTimeFilter = document.querySelector("select[name='sort-beat-time']");
    let scoreFilter = document.querySelector("select[name='sort-game-score']");
    let completionStatusFilter = document.querySelector("select[name='completion-status']");
    let favStatusFilter = document.querySelector("select[name='favorite-status']");
    let pageSize = document.querySelector("select[name='page-size']").value;
    let gameTitle = document.querySelector("input[name='keywords']").value;
    
    if(completionStatusFilter.value > 0){
        switch (completionStatusFilter.value){
            case '1': completionStatus = 'incomplete'; break;
            case '2': completionStatus = 'completed'; break;
        }
    }

    if(favStatusFilter.value > 0){
        switch (favStatusFilter.value){
            case '1': favStatus = 'favorite'; break;
            case '2': favStatus = 'non-favorite'; break;
        }
    }
    
    // ordering
    if(releaseTimeFilter.value > 0){
        sortBy = 'release_date';
        switch (releaseTimeFilter.value){
            case '1': sortType = 'desc'; break;
            case '2': sortType = 'asc'; break;
        }        
    } 
    
    if (beatTimeFilter.value > 0){
        sortBy = 'beat_time';
        switch (beatTimeFilter.value){
            case '1': sortType = 'desc'; break;
            case '2': sortType = 'asc'; break;
        }
    }
    
    if(scoreFilter.value > 0){
        sortBy = 'game_score';
        switch (scoreFilter.value){
            case '1': sortType = 'desc'; break;
            case '2': sortType = 'asc'; break;
        }
    }
    
    const filterData = {
        beatTimeInitial: timeRange[0],
        beatTimeFinal: timeRange[1],
        genres: filterGenres.join(','),
        sortBy: sortBy, 
        sortType: sortType,
        page: pageNumber,
        pageSize: pageSize,
        gameTitle: gameTitle,
        completionStatus: completionStatus,
        favoriteStatus: favStatus
    };
    
    const bodyData = toUrlEncoded(filterData);
    try {
        let response = await fetch(`${url}/catalog/filter`, {
            method: 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: bodyData
        });

        await toggleLoader();

        if (response.status === 200) {
            let gameCards = await response.text();
            if(gameCards.length > 0){
                let container = document.querySelector('#result-container');
                container.innerHTML = '';
                container.innerHTML = gameCards;
            }
            return;
        } else {
            let textResult = await response.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}

function resetCatalogFilters(){
    let url = location.origin;
    window.location = `${url}/catalog`
}

async function editFromCatalog(btnAction, steamAppId) {
    let url = location.origin;
    $.confirm({
        title: 'Edit Details!',
        theme: 'dark',
        type: 'purple',
        content: `url:${url}/catalog/edit?appId=${steamAppId}`,
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

async function editCatalog(btnAction) {
    let action = btnAction.dataset.action;
    let steamAppId = btnAction.dataset.steamid;
    let btnType = btnAction.dataset.btntype;
    
    if (action === 'save') {
        await saveToCatalog(btnAction, steamAppId);
    } else if (action === 'remove') {
        await removeFromCatalog(btnAction, steamAppId, btnType);
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
                btnAction.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><path d="M112,112l20,320c.95,18.49,14.4,32,32,32H348c17.67,0,30.87-13.51,32-32l20-320" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:32px"></path><line x1="80" y1="112" x2="432" y2="112" style="stroke-linecap:round;stroke-miterlimit:10;stroke-width:32px"></line><path d="M192,112V72h0a23.93,23.93,0,0,1,24-24h80a23.93,23.93,0,0,1,24,24h0v40" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:32px"></path><line x1="256" y1="176" x2="256" y2="400" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:32px"></line><line x1="184" y1="176" x2="192" y2="400" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:32px"></line><line x1="328" y1="176" x2="320" y2="400" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:32px"></line></svg>Remove';
                btnAction.className = 'btn-edit-static-wide details__favorite';                
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
            let textResult = await response.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
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
            '\t\t\t\t<div class="col-md-12 mt-5 d-flex justify-content-center">\n' +
            '\t\t\t\t\t<i class="fa fa-search not-found-icon" aria-hidden="true"></i>\n' +
            '\t\t\t\t\t<span class="not-found-text">Empty</span>\n' +
            '\t\t\t\t</div>\n' +
            '\t\t\t</div>\n' +
            '\t\t\t<div class="row d-flex justify-content-center">\n' +
            '\t\t\t\t<div class="col-md-3">\n' +
            '\t\t\t\t\t<a class="sign__btn" asp-controller="Home" asp-action="New">Add New</a>\n' +
            '\t\t\t\t</div>\n' +
            '\t\t\t</div>';
    } else {
        catalogCards.forEach((card) => {
            container.append(card);
        });
        container.append(pagination);        
    }    
}

async function removeFromCatalog(btnAction, steamAppId, btnType){
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
                            method : 'delete',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                            },
                            body: bodyData
                        });

                        await toggleLoader();

                        if(response.status === 200){
                            // switch button
                            if(btnType !== 'card'){
                                btnAction.dataset.action = 'save';
                                btnAction.innerHTML = 'Add';
                                btnAction.className = 'details__buy';

                                // hide fav button
                                let btnFav = document.querySelectorAll('.details__favorite');
                                if(btnFav !== null){
                                    Array.from(btnFav).forEach((item) => {
                                        item.style.display = 'none';
                                    });
                                }

                                // go back to catalog
                                window.location = `${url}/catalog`;
                                return;
                            }
                            
                            // remove game card
                            await removeGameCard(steamAppId);
                            
                            showToastr("Success!", "Game removed successfully from your catalog", "success");
                            return;
                        } else {
                            let textResult = await response.text();
                            showToastr("Error!", textResult, "error");                            
                            return;   
                        }                            
                    } catch (e) {
                        console.log(e);
                    }

                    await toggleLoader();
                    showToastr("Warning!", "Something weird happened!", "warning");
                }
            },
            cancel: {
                btnClass: 'btn-dlg-cancel'                
            }            
        }
    });
}

async function removeFavorite(appId){
    $.confirm({
        title: 'Remove Favorite!',
        content: 'Remove this game from your favorites list?',
        theme: 'dark',
        type: 'purple',
        closeIcon: false,
        buttons: {
            ok: {
                btnClass: 'btn-dlg-confirm',
                action: async function () {
                    await toggleLoader();
                    let url = location.origin;
                    const gameId = {appId: appId};
                    const bodyData = toUrlEncoded(gameId);
                    try {
                        let response = await fetch(`${url}/catalog/favorite`, {
                            method : 'delete',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                            },
                            body: bodyData
                        });

                        await toggleLoader();

                        if(response.status === 200){
                            let newTable = await response.text();
                            let favsContainer = document.querySelector('#favorite-games-container');
                            favsContainer.innerHTML = newTable;
                            let favTable = $('#favorite-games-table');
                            if (favTable !== null) {
                                favTable.DataTable({
                                    "order": [], // disable init ordering
                                    responsive: true
                                });
                            }
                            showToastr("Success!", 'Game removed from your favorites list', "success");
                            return;
                        } else {
                            let textResult = await response.text();
                            showToastr("Error!", textResult, "error");
                            return;
                        }
                    } catch (e) {
                        console.log(e);
                    }

                    await toggleLoader();
                    showToastr("Warning!", "Something weird happened!", "warning");
                }
            },
            cancel: {
                btnClass: 'btn-dlg-cancel'
            }
        }
    });
}

async function addToFavorites(button){
    let appId = button.dataset.steamid;
    let btnType = button.dataset.btntype;

    await toggleLoader();
    let url = location.origin;
    const gameId = {appId: appId};
    const bodyData = toUrlEncoded(gameId);
    try {
        let response = await fetch(`${url}/catalog/favorite`, {
            method : 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: bodyData
        });

        await toggleLoader();

        if(response.status === 200){
            let message;       
            let coverPills;
            let btnFav;
            
            if(btnType === 'card'){
                btnFav = button;
                coverPills = button.parentElement.parentElement.querySelector('.card__cover > .cover-pills')
            } else {
                coverPills = document.querySelector('div.cover-pills');
                btnFav = document.querySelector('.details__actions > #btn-fav');
            }

            let currentFav = btnFav.classList.contains('pressed');
            if(currentFav){
                btnFav.classList.remove('pressed');
                let favPill = coverPills.querySelector('.card__preorder.fav');
                if(favPill !== null) coverPills.removeChild(favPill);
                message = 'Game removed from your favorites';
            } else {
                btnFav.classList.add('pressed');
                let favPill = createElementFromHTML('<span class="card__preorder fav">FAV</span>');
                coverPills.appendChild(favPill);
                message = 'Game added to your favorites';
            }
            
            showToastr("Success!", message, "success");
            return;
        } else {
            let textResult = await response.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}

async function removeCompleted(appId){
    $.confirm({
        title: 'Remove Completed!',
        content: 'Remove this game from your completed games list?',
        theme: 'dark',
        type: 'purple',
        closeIcon: false,
        buttons: {
            ok: {
                btnClass: 'btn-dlg-confirm',
                action: async function () {
                    await toggleLoader();
                    let url = location.origin;
                    const gameId = {appId: appId};
                    const bodyData = toUrlEncoded(gameId);
                    try {
                        let response = await fetch(`${url}/catalog/played`, {
                            method : 'delete',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                            },
                            body: bodyData
                        });

                        await toggleLoader();

                        if(response.status === 200){
                            let newTable = await response.text();
                            let playedContainer = document.querySelector('#played-games-container');
                            playedContainer.innerHTML = newTable;
                            let playedTable = $('#completed-games-table');
                            if (playedTable !== null) {
                                playedTable.DataTable({
                                    "order": [], // disable init ordering
                                    responsive: true
                                });
                            }
                            showToastr("Success!", 'Game removed from your completed list', "success");
                            return;
                        } else {
                            let textResult = await response.text();
                            showToastr("Error!", textResult, "error");
                            return;
                        }
                    } catch (e) {
                        console.log(e);
                    }

                    await toggleLoader();
                    showToastr("Warning!", "Something weird happened!", "warning");
                }
            },
            cancel: {
                btnClass: 'btn-dlg-cancel'
            }
        }
    });
}

async function addToCompleted(button){
    let appId = button.dataset.steamid;
    let btnType = button.dataset.btntype;
    
    await toggleLoader();
    let url = location.origin;
    const gameId = {appId: appId};
    const bodyData = toUrlEncoded(gameId);
    try {
        let response = await fetch(`${url}/catalog/played`, {
            method : 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: bodyData
        });

        await toggleLoader();

        if(response.status === 200){
            let message;
            let coverPills;
            let btnPlayed;

            if(btnType === 'card'){
                btnPlayed = button;
                coverPills = button.parentElement.parentElement.querySelector('.card__cover > .cover-pills')
            } else {
                coverPills = document.querySelector('div.cover-pills');
                btnPlayed = document.querySelector('.details__actions > #btn-played');
            }

            let currentFav = btnPlayed.classList.contains('pressed');
            if(currentFav){
                btnPlayed.classList.remove('pressed');
                let playedStripe = coverPills.querySelector('.played');
                if(playedStripe !== null) coverPills.removeChild(playedStripe);
                message = 'Game removed from your completed list';
            } else {
                btnPlayed.classList.add('pressed');
                let playedStripe = createElementFromHTML('<span class="played">PLAYED</span>');
                coverPills.appendChild(playedStripe);
                message = 'Game added to your completed list';
            }

            showToastr("Success!", message, "success");
            return;
        } else {
            let textResult = await response.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}

const toUrlEncoded = obj => 
    Object.keys(obj).map(k => encodeURIComponent(k) + '=' + encodeURIComponent(obj[k])).join('&');

function createElementFromHTML(htmlString) {
    let div = document.createElement('div');
    div.innerHTML = htmlString.trim();

    // Change this to div.childNodes to support multiple top-level nodes
    return div.firstChild;
}

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

async function userLogout() {
    await toggleLoader();
    let url = location.origin;
    try {
        let result = await fetch(`${url}/account/logout`, {
            method: 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            }
        });
        
        if(result.status === 200){
            window.location = `${url}/account/login`
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}

async function userRemove() {
    $.confirm({
        title: 'Remove Account!',
        content: 'This is a permanent action. Everything related to this account will be deleted',
        theme: 'dark',
        type: 'purple',
        closeIcon: false,
        buttons: {
            ok: {
                btnClass: 'btn-dlg-confirm',
                action: async function () {
                    await toggleLoader();
                    let url = location.origin;
                    try {
                        let result = await fetch(`${url}/account/delete`, {
                            method: 'post',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                            }
                        });
                        
                        if(result.status === 200){
                            window.location = `${url}/account/login`
                            return;
                        }
                    } catch (e) {
                        console.log(e);                        
                    }

                    await toggleLoader();
                    showToastr("Warning!", "Something weird happened!", "warning");
                }
            },
            cancel: {
                btnClass: 'btn-dlg-cancel'
            }
        }
    });
}

async function updateSteamDb(e) {
    e.preventDefault();
    await toggleLoader();
    let url = location.origin;
    try {
        let result = await fetch(`${url}/catalog/updatedb`);
        await toggleLoader();
        
        if (result.status === 200) {
            showToastr("Success!", 'Steam database updated successfully', "success");
            return;
        } else {
            let textResult = await result.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (error) {
        console.log(error);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}

async function saveProfileDetails(e) {
    e.preventDefault();
    let detailsForm = document.querySelector('form#details-form');    
    const formData = new URLSearchParams(new FormData(detailsForm)).toString();
    await toggleLoader();
    let url = location.origin;
    try {
        let result = await fetch(`${url}/profile/savedetails`, {
            method: 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: formData
        });

        await toggleLoader();
        
        if (result.status === 200) {
            showToastr("Success!", 'Profile edited successfully', "success");
            return;
        } else {
            let textResult = await result.text();
            showToastr("Error!", textResult, "error");
            // TODO: append errors in a div 
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}

async function saveProfileSecurity(e) {
    e.preventDefault();
    let securityForm = document.querySelector('form#security-form');
    const formData = new URLSearchParams(new FormData(securityForm)).toString();
    await toggleLoader();
    let url = location.origin;
    try {
        let result = await fetch(`${url}/profile/savesecurity`, {
            method: 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: formData
        });

        await toggleLoader();

        if (result.status === 200) {
            showToastr("Success!", 'Profile edited successfully', "success");
            let form = document.querySelector('#security-form');
            let inputs = form.querySelectorAll('input');
            inputs.forEach((e) => {
                e.value = '';
            });
            return;
        } else {
            let textResult = await result.text();
            showToastr("Error!", textResult, "error");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Warning!", "Something weird happened!", "warning");
}