# Integrating GET /api/accommodations/my-bookings

This document explains how a frontend application can call the `GET /api/accommodations/my-bookings` endpoint to retrieve bookings across all accommodations owned by the current authenticated user.

## Overview

- Endpoint: `GET /api/accommodations/my-bookings`
- Purpose: Returns paginated bookings for all accommodations belonging to the authenticated user (owner/manager).
- Authentication: Bearer token (JWT) required. The token must contain the user's Id in the `sub`/NameIdentifier claim and the `Hotel` role (or `Admin`).

## Authorization

The endpoint enforces that the caller is authenticated and has the `Hotel` role (or `Admin`). If the caller is not authenticated, the endpoint returns `401 Unauthorized`. If the caller is authenticated but lacks the required role, the endpoint returns `403 Forbidden`.

Include the Authorization header:

Authorization: `Bearer <access_token>`

## Query parameters

All parameters are optional unless specified.

- `pageNumber` (int, default: 1) — 1-based page number to fetch.
- `pageSize` (int, default: 20) — Items per page. The server caps pages to reasonable limits (e.g. 100).
- `status` (string, optional) — Booking status filter. Use one of the API's BookingStatus values, e.g. `Confirmed`, `Reserved`, `Cancelled`, `CheckedIn`.
- `paymentStatus` (string, optional) — Payment status filter. Use PaymentStatus enum values, e.g. `Paid`, `Pending`, `Refunded`.
- `checkInDateFrom` (date string, optional) — ISO date (yyyy-MM-dd or full ISO). Filters bookings with CheckInDate >= this value.
- `checkInDateTo` (date string, optional) — ISO date (yyyy-MM-dd or full ISO). Filters bookings with CheckInDate <= this value.

Examples:

- Get the first page with default size:
  - `/api/accommodations/my-bookings`
- Get page 2 with 50 items per page:
  - `/api/accommodations/my-bookings?pageNumber=2&pageSize=50`
- Get confirmed, paid bookings in a date range:
  - `/api/accommodations/my-bookings?status=Confirmed&paymentStatus=Paid&checkInDateFrom=2025-09-01&checkInDateTo=2025-09-30`

## Successful response

Status: 200 OK

Response body (JSON):

{
"Items": [
{
"Id": 123,
"BookingReference": "ABC-12345",
"RoomId": 12,
"RoomName": "Deluxe Suite",
"CheckInDate": "2025-09-20T00:00:00",
"CheckOutDate": "2025-09-22T00:00:00",
"NumberOfGuests": 2,
"NumberOfNights": 2,
"BaseAmount": 200.0,
"TaxAmount": 20.0,
"ServiceFee": 5.0,
"TotalAmount": 225.0,
"Status": "Confirmed",
"PaymentStatus": "Paid",
"GuestName": "Jane Doe",
"GuestEmail": "jane@example.com",
"GuestPhone": "+1234567890",
"SpecialRequests": "Late check-in",
"CreatedAt": "2025-09-01T14:22:00Z",
"UpdatedAt": "2025-09-05T09:10:00Z",
"ActualCheckInAt": null,
"ActualCheckOutAt": null,
"Accommodation": {
"Id": 45,
"Name": "Seaside Hotel",
"Description": "A cozy hotel by the sea",
"Logo": "https://cdn.example.com/logos/seaside.png",
"Address": "123 Ocean Drive",
"EmailAddress": "contact@seaside.example",
"ContactNo": "+11234567890",
"IsActive": true,
"Status": "Active",
"ActiveRoomCount": 12
}
}
],
"TotalCount": 250,
"PageNumber": 1,
"PageSize": 20,
"TotalPages": 13,
"HasNextPage": true,
"HasPreviousPage": false
}

Notes:

- Dates/times are returned in ISO 8601 format. The exact timezone depends on how the server stores dates — treat them as UTC unless your server uses local offsets.
- `Items` contains objects shaped like `AccommodationBookingDto` in the server code.

## Error responses

- `401 Unauthorized` — Missing or invalid token.
- `403 Forbidden` — Authenticated but not in `Hotel` or `Admin` role.
- `500 Internal Server Error` — Unexpected server error.

Error body format (example):

{
"message": "An error occurred while retrieving bookings"
}

## Example usage (fetch in a React app)

JavaScript (using fetch):

```js
async function fetchMyBookings({
  token,
  pageNumber = 1,
  pageSize = 20,
  filters = {},
}) {
  const params = new URLSearchParams({ pageNumber, pageSize });

  if (filters.status) params.set("status", filters.status);
  if (filters.paymentStatus) params.set("paymentStatus", filters.paymentStatus);
  if (filters.checkInDateFrom)
    params.set("checkInDateFrom", filters.checkInDateFrom);
  if (filters.checkInDateTo) params.set("checkInDateTo", filters.checkInDateTo);

  const res = await fetch(
    `/api/accommodations/my-bookings?${params.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    }
  );

  if (res.status === 401) throw new Error("Not authenticated");
  if (res.status === 403) throw new Error("Forbidden: requires Hotel role");
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: "Unknown error" }));
    throw new Error(err.message || "Request failed");
  }

  return res.json();
}
```

## Example curl

```bash
curl -H "Authorization: Bearer $ACCESS_TOKEN" \
  "https://api.example.com/api/accommodations/my-bookings?pageNumber=1&pageSize=20"
```

## Frontend concerns and tips

- Pagination: prefer incremental loading (infinite scroll) or explicit page navigation for large result sets.
- Filtering: validate and normalize date inputs client-side before sending them to the server.
- Timezones: display dates in the user's local timezone, converting from the server's ISO timestamps.
- Role awareness: the frontend can hide or show the bookings UI based on the user's roles present in their token (e.g., show bookings only for `Hotel`/`Admin`).
- Error handling: gracefully surface `401`/`403` to the user (prompt login, show 'insufficient privileges' message).

## Response contract (concise)

- Request:
  - Headers: Authorization: Bearer <token>
  - Query params: pageNumber, pageSize, status, paymentStatus, checkInDateFrom, checkInDateTo
- Response (200): JSON object with properties: Items[], TotalCount, PageNumber, PageSize, TotalPages, HasNextPage, HasPreviousPage
- Each item in `Items[]` now includes an optional `Accommodation` object with `AccommodationSummaryDto` fields: Id, Name, Description, Logo, Address, EmailAddress, ContactNo, IsActive, Status, ActiveRoomCount.
- Errors: 401, 403, 500

---

If you'd like, I can:

- Add a typed TypeScript client (API wrapper) for this endpoint.
- Create a small React component that loads and displays the bookings using the endpoint.
