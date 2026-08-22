import apiClient
    from '../api/apiClient'

// ============================================================
// REPORT SERVICE
// ============================================================

function buildParams(
    from,
    to
) {

    const params =
        {}

    if (from) {

        params.from =
            from
    }

    if (to) {

        params.to =
            to
    }

    return params
}

// ============================================================
// GET DASHBOARD
// ============================================================

export async function getReportDashboard(
    from = null,
    to = null
) {

    const response =
        await apiClient.get(
            '/admin/reports',
            {
                params:
                    buildParams(
                        from,
                        to
                    )
            }
        )

    return response.data
}

// ============================================================
// DOWNLOAD FILE
// ============================================================

async function downloadReport(
    type,
    defaultFileName,
    from = null,
    to = null
) {

    const response =
        await apiClient.get(
            `/admin/reports/export/${type}`,
            {
                params:
                    buildParams(
                        from,
                        to
                    ),

                responseType:
                    'blob'
            }
        )

    // ========================================================
    // READ FILE NAME FROM HEADER
    // ========================================================

    const contentDisposition =
        response.headers[
        'content-disposition'
        ]

    let fileName =
        defaultFileName

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

    // ========================================================
    // DOWNLOAD
    // ========================================================

    const url =
        window.URL.createObjectURL(
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
// CSV
// ============================================================

export async function exportReportCsv(
    from = null,
    to = null
) {

    await downloadReport(
        'csv',
        'business-report.csv',
        from,
        to
    )
}

// ============================================================
// EXCEL
// ============================================================

export async function exportReportExcel(
    from = null,
    to = null
) {

    await downloadReport(
        'excel',
        'business-report.xlsx',
        from,
        to
    )
}

// ============================================================
// PDF
// ============================================================

export async function exportReportPdf(
    from = null,
    to = null
) {

    await downloadReport(
        'pdf',
        'business-report.pdf',
        from,
        to
    )
}