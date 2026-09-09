// EVIA Site-wide JavaScript with HCI Interactivity

$(document).ready(function () {
    // Auto-dismiss toast notifications after 4 seconds
    setTimeout(function () {
        $('.toast-item').fadeOut(400, function () { $(this).remove(); });
    }, 4000);

    // Global Search Autocomplete
    var searchInput = $('#globalSearchInput');
    var suggestionsBox = $('#searchSuggestions');
    var debounceTimer;

    searchInput.on('input', function () {
        var query = $(this).val().trim();
        clearTimeout(debounceTimer);

        if (query.length < 2) {
            suggestionsBox.hide().empty();
            return;
        }

        debounceTimer = setTimeout(function () {
            $.ajax({
                url: '/Shop/SearchSuggestions',
                data: { q: query },
                dataType: 'json',
                success: function (data) {
                    suggestionsBox.empty();
                    if (data && data.length > 0) {
                        $.each(data, function (i, item) {
                            var img = item.imageUrl || '/images/product-placeholder.jpg';
                            var html = `
                                <div class="search-suggestion-item" onclick="window.location.href='/Shop/Details/${item.id}'">
                                    <img src="${img}" alt="${item.name}">
                                    <div>
                                        <div style="font-weight:600; font-size:14px; color:white;">${item.name}</div>
                                        <div style="font-size:12px; color:#e5b869;">Rs. ${item.price.toLocaleString()} • ${item.category}</div>
                                    </div>
                                </div>
                            `;
                            suggestionsBox.append(html);
                        });
                        suggestionsBox.show();
                    } else {
                        suggestionsBox.append('<div style="padding:12px; font-size:13px; color:#94a3b8; text-align:center;">No matching fashion items found</div>').show();
                    }
                }
            });
        }, 250);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.search-box-container').length) {
            suggestionsBox.hide();
        }
    });
});

function showToast(message, type) {
    var icon = type === 'error' ? 'fa-circle-xmark' : 'fa-circle-check';
    var color = type === 'error' ? 'var(--accent-rose)' : '#10b981';
    var toastHtml = `
        <div class="toast-item">
            <i class="fa-solid ${icon}" style="color:${color}; font-size:20px;"></i>
            ${message}
        </div>
    `;
    var $toast = $(toastHtml).appendTo('#toastContainer');
    setTimeout(function () {
        $toast.fadeOut(400, function () { $(this).remove(); });
    }, 4000);
}

