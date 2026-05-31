$(function () {
    const modalContainer = $('#modal-container');
    const modalBody = $('#modal-body-content');
    const modalTitle = $('#modal-title');
    const modalDialog = modalContainer.find('.modal-dialog');
    const toaster = new ToasterUi();

    if (modalContainer.length) {
        const bootstrapModal = new bootstrap.Modal(modalContainer[0]);

        $(document).on('click', '[data-ajax-modal="true"]', function (e) {
            e.preventDefault();
            const url = $(this).attr('href') || $(this).data('url');
            const title = $(this).data('modal-title') || 'Popup Form';
            const size = $(this).data('modal-size') || '';

            if (!url) return;

            modalDialog.removeClass('modal-sm modal-lg modal-xl').addClass(size);
            modalTitle.text(title);

            modalBody.html(`
                <div class="text-center py-5">
                    <div class="spinner-border text-primary" style="width: 3rem; height: 3rem;" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="mt-2 text-secondary font-weight-medium">Retrieving form, please wait...</div>
                </div>
            `);

            bootstrapModal.show();

            $.get(url)
                .done(function (response) {
                    modalBody.html(response);
                    rebindValidation();
                })
                .fail(function (xhr) {
                    modalBody.html(`
                        <div class="alert alert-danger-custom d-flex flex-column align-items-center gap-2 text-center p-4" role="alert">
                            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" class="bi bi-exclamation-triangle-fill text-danger mb-2" viewBox="0 0 16 16">
                                <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/>
                            </svg>
                            <h5 class="text-dark font-weight-bold">Failed to Load Content</h5>
                            <p class="text-secondary mb-0">The server encountered an error retrieving this form. Please try again.</p>
                        </div>
                    `);
                });
        });

        $(document).on('submit', '#modal-body-content form', function (e) {
            const form = $(this);
            e.preventDefault();

            if (form.data('validator') && !form.valid()) {
                return false;
            }

            const action = (form.attr('action') || '').toLowerCase();
            const operationType = form.data('operation') ||
                (action.includes('delete') ? 'delete' :
                    action.includes('edit') || action.includes('update') ? 'update' : 'create');

            const submitBtn = form.find('[type="submit"]');
            const originalBtnHtml = submitBtn.html();
            submitBtn.prop('disabled', true).html(`
                <span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                Processing...
            `);

            $.ajax({
                url: form.attr('action'),
                type: form.attr('method') || 'POST',
                data: form.serialize()
            })
                .done(function (response, status, xhr) {
                    const responseUrl = xhr.responseURL;
                    let redirectHappened = false;

                    if (responseUrl) {
                        const cleanResponseUrl = responseUrl.split('?')[0].split('#')[0];
                        const cleanFormAction = (form.attr('action') || '').split('?')[0].split('#')[0];

                        const tempAnchor = document.createElement('a');
                        tempAnchor.href = cleanFormAction;
                        const absoluteFormAction = tempAnchor.href;

                        if (cleanResponseUrl !== absoluteFormAction) {
                            redirectHappened = true;
                        }
                    } else {
                        if (response.indexOf('class="table-responsive"') !== -1 || response.indexOf('Student Management') !== -1) {
                            redirectHappened = true;
                        }
                    }

                    if (redirectHappened) {
                        bootstrapModal.hide();

                        const successMessages = {
                            create: 'Data saved successfully.',
                            update: 'Data updated successfully.',
                            delete: 'Data deleted successfully.'
                        };
                        toaster.addToast(successMessages[operationType] || 'Operation completed successfully.', 'success', {
                            duration: 4000,
                            autoClose: true
                        });
                        setTimeout(() => { window.location.reload(); }, 2000);

                   
                    } else {
                        modalBody.html(response);
                        rebindValidation();
                    }
                })
                .fail(function (xhr) {
                    toaster.addToast('An error occurred while saving. Please try again.', 'error', {
                        duration: 5000,
                        autoClose: true
                    });

                    const alertBox = modalBody.find('.alert-danger-custom');
                    if (alertBox.length) {
                        alertBox.remove();
                    }
                    modalBody.prepend(`
                    <div class="alert alert-danger-custom mb-3 p-3 text-sm d-flex align-items-center gap-2">
                        <span>An error occurred while saving. Please check the inputs and try again.</span>
                    </div>
                `);
                })
                .always(function () {
                    submitBtn.prop('disabled', false).html(originalBtnHtml);
                });
        });

        $(document).on('click', '#modal-body-content [data-bs-dismiss="modal"]', function (e) {
            e.preventDefault();
        });

        function rebindValidation() {
            const form = modalBody.find('form');
            if (form.length && typeof $.validator !== 'undefined' && typeof $.validator.unobtrusive !== 'undefined') {
                form.removeData("validator").removeData("unobtrusiveValidation");
                $.validator.unobtrusive.parse(form);
            }
        }
    }
});