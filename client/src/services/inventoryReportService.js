import apiClient
    from '../api/apiClient'

// ============================================================
// GET INVENTORY REPORT
// ============================================================

export async function getInventoryReport(
    threshold = 5
) {

    const response =
        await apiClient.get(
            '/admin/inventory-report',
            {
                params: {
                    threshold
                }
            }
        )

    return response.data
}

// ============================================================
// DOWNLOAD
// ============================================================

async function downloadInventoryReport(
    type,
    threshold
) {

    const response =
        await apiClient.get(
            `/admin/inventory-report/export/${type}`,
            {
                params: {
                    threshold
                },

                responseType:
                    'blob'
            }
        )

    const contentDisposition =
        response.headers[
        'content-disposition'
        ]

    let extension =
        type

    if (type === 'excel') {
        extension =
            'xlsx'
    }

    let fileName =
        `inventory-report.${extension}`

    if (contentDisposition) {

        const match =
            contentDisposition.match(
                /filename="?([^"]+)"?/i
            )

        if (
            match &&
            match[1]
        ) {

            fileName =
                match[1]
        }
    }

    const url =
        window.URL
            .createObjectURL(
                response.data
            )

    const link =
        document.createElement(
            'a'
        )

    link.href =
        url

    link.download =
        fileName

    document.body.appendChild(
        link
    )

    link.click()

    link.remove()

    window.URL.revokeObjectURL(
        url
    )
}

// ============================================================
// EXCEL
// ============================================================

export async function exportInventoryExcel(
    threshold = 5
) {

    await downloadInventoryReport(
        'excel',
        threshold
    )
}

// ============================================================
// CSV
// ============================================================

export async function exportInventoryCsv(
    threshold = 5
) {

    await downloadInventoryReport(
        'csv',
        threshold
    )
}

// ============================================================
// PDF
// ============================================================

export async function exportInventoryPdf(
    threshold = 5
) {

    await downloadInventoryReport(
        'pdf',
        threshold
    )
}