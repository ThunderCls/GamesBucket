// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function applyFilter(){
    
    let filterKeyword = document.querySelector("input[name='keywords']").value;
    let filterGenres = document.querySelector("select[name='genres']");
    let sortType = Number(document.querySelector("select[name='sort']").value);    
    let container = document.querySelector('#search-result-container');
    
    container.querySelectorAll('div.game-card').forEach((item) =>{
        if(item != null){
            let title = item.querySelector('div.card__title > h3 > a');
            let genres = item.dataset.genre.split(';');
            let filterGenreText = filterGenres.querySelector(`option[value='${filterGenres.value}']`).innerText

            if(title.innerHTML.toLowerCase().includes(filterKeyword.toLowerCase())){
                if(filterGenreText.toLowerCase() === 'all genres'
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

    if(sortType >= 0){
        let cards = container.querySelectorAll("div.game-card");
        let pagination = document.querySelector('#search-result-container > div:last-child');
        let sortedCards = Array.from(cards).sort((a, b) => {
            let dateA = new Date(Number(a.dataset.releaseDate));
            let dateB = new Date(Number(b.dataset.releaseDate));
            switch (sortType){
                case 1: return dateB - dateA;
                case 2: return dateA - dateB;
                default: return 0;
            }            
        });

        container.innerHTML = '';
        sortedCards.forEach((card) => {
            container.append(card);    
        });
        container.append(pagination);        
    }
}

async function saveToCatalog(steamAppId){
    await toggleLoader();
    //let url = window.location.protocol + "//" + location.host.split(":")[0];
    let url = location.origin;
    const gameId = { appId: steamAppId };
    const bodyData = toUrlEncoded(gameId);    
    try {
        let response = await fetch(`${url}/catalog/addgame`, {
            method : 'post',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: bodyData
        });

        await toggleLoader();
        
        if(response.status === 201){
            showToastr("Success!", "Element successfully added", "success");
            return;
        }
    } catch (e) {
        console.log(e);
    }

    await toggleLoader();
    showToastr("Error!", "An error occured while adding the game", "error");
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
    if(loader.style.visibility !== 'visible'){
        loader.style.visibility = 'visible';
    } else {
        loader.style.visibility = 'hidden';
    }
}