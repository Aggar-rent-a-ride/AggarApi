# AggarApi


## 📋 Table of Contents

- [🎯 Overview](#-overview)
- [✨ Features](#-features)
- [🛠 Technologies](#-technologies)
- [🏗️ Architecture](#-architecture)
- [🗄️ Database Schema](#-database-schema)
- [🧪 Testing](#-testing)
- [🎮 Demo](#-demo)
- [📚 API Documentation](#-api-documentation)
- [📦 Installation](#-installation)
- [⚙️ Environment Configuration](#-environment-configuration)
- [📞 Contact](#-contact)


---


## 🎯 Overview
Aggar API is a scalable and robust backend that powers a full-featured Aggar app. It connects vehicle owners (renters) with customers looking to book vehicles, handling all core operations, including vehicle listings, booking management, user accounts, and real-time messaging and notifications.

The system ensures secure transactions through escrow payment system, holding funds until rentals are confirmed. An integrated admin dashboard enables management of reports, users, and platform analytics, supporting safe and transparent operations.

Designed for reliability, the backend supports real-time communication, role-based access control, and a structured end-to-end booking flow.

---


## ✨ Features

### 🔐 Authentication
- **Secure registration with email verification:** Users must confirm their email using a code sent to their inbox before gaining access.

- **JWT-based authentication:** Short-lived access tokens and long-lived refresh tokens power a stateless login system.

- **Token refresh endpoint:** Keeps users logged in securely without frequent re-authentication.

- **Protected endpoints:** Sensitive routes are secured based on authentication and role level.



### 👥 User Management

- **Role-Based Access**: Supports three roles Customer, Renter, and Admin, each with different permissions and access levels.

- **Profile Management**: Users can view and update their profile details, including personal information and profile image.

- **Activity History**: Users can access their full history, including booking records, rental activity, and reviews they've written.

- **Submit reports**: Users can report inappropriate behavior or content (e.g., reviews, vehicles, users, messages).

- **Review system**: During rentals, customers and renters can leave reviews and ratings for each other.


### 👤 Customer Features

- **Saved Vehicles**: Customers can save vehicles to their favorites list and access them later from their profile.

- **Browse & Search Vehicles**: Customers can explore available vehicles, prioritized by proximity to their location. The search supports advanced filtering (by location, price, brand, model, vehicle type, and more) to help users find the right vehicle quickly and easily.


### 👤 Renter Features

- **Vehicle Management**: Renters can add, edit, or remove vehicles from their listings. Each vehicle includes detailed information such as model, brand, type, color, seating capacity, rental price, and a descriptive overview.

- **Custom Discounts**: Renters can offer discounts based on rental duration (e.g., reduced prices for bookings longer than a specific number of days) per vehicle.

- **Booking Requests**: Renters receive incoming booking requests from customers and can either accept or reject them through a structured approval process.

- **Availability Calendar**: Renters can view a calendar that highlights booked days across all their vehicles, making it easy to track availability and manage future bookings.


### 🛡️ Admin Dashboard

- **Report Management**: View and respond to user-submitted reports related to inappropriate content or behavior.

- **User Moderation**: Issue warnings to users for violating platform rules.

- **Role Distribution Insights**: Visual analytics showing percentages of Customers and Renters.

- **Platform Earnings Overview**: Track revenue generated from booking commissions and platform fees.

- **Manage Vehicle Brands & Types**: Admins can add, edit, or remove vehicle brands and types to maintain a clean and accurate selection list for users.


### 🚗 Vehicle Management

#### 1. Add & Manage Vehicles  
Renters can add, edit, or remove their vehicles.

Each vehicle includes:
- **Basic Info**: Brand, model, year, color, type, etc..
- **Specifications**: Transmission type, number of seats
- **Pricing**: Daily rental price
- **Overview**: Short written details about the vehicle
- **Photos**: One required mian image and optional multiple image uploads to showcase the vehicle

#### 2. Location Tagging  
Renters must set a pickup location when registering a vehicle.  
- The location is used for:
  - **Search filters** so customers can find the nearest available vehicles
  - **Booking coordination**, helping customers easily reach the pickup point

#### 3. Discount Rules  
Renters can configure **dynamic discounts** based on the number of booking days:
- Examples:  
  - 5% off for rentals of 3 days or more  
  - 10% off for rentals of 7 days or more



### 📅 Booking & Rental Process

#### 1. Initiate Booking
- The customer selects a vehicle and specifies:
  - **Start Date & Time**
  - **End Date & Time**

- The system checks vehicle availability:
  - If **unavailable**, the request is rejected immediately.
  - If **available**, the system responds with:
    - Total rental duration (in days)
    - Total price
    - Any applicable **discounts**


#### 2. Renter Response
- The booking request is sent to the **renter**.
- The renter can:
  - **Accept** or **Reject** the booking.
- If the renter does **not respond before the start date**, the booking is **automatically canceled**.
- The customer can **cancel** the request at any time before payment.


#### 3. Customer Payment
- If the renter **accepts**, the customer must:
  - **Confirm and complete payment** within **3 days** of acceptance.
- Upon successful payment:
  - A **rental record** is created.
  - The **payment is held** securely in an **escrow** system.
  - The renter receives a **QR code** via email to verify the rental start.
- **Platform Fee**:  
  The platform deducts a service fee from each successful booking.


#### 4. Cancellations & Refund
- **Before payment**: The customer can cancel the booking at any time without penalty.
- **After payment and before QR confirmation**: 
  - If the customer cancels, a **penalty fee** may be applied to compensate the renter.
- **After QR confirmation**: 
  - No refunds are issued, as the rental is considered started and confirmed.


#### 5. Rental Confirmation (QR Code Scan)
- On the rental start date:
  - The customer and renter meet in person.
  - The customer **scans the QR code** to confirm handover.
- Once scanned:
  - The **payment is released** from escrow to the renter’s account.


  
### ⭐ Review and Rating System

After the rental, both customers and renters can review each other.  
The review system uses both **shared** and **role-specific** rating criteria to ensure fairness and context-aware feedback.  
Each user submits a review and star ratings:

- **Shared Rating Criteria**:
  - **Punctuality**: Timeliness during pickup and drop-off
  - **Behavior**: Respectful and cooperative communication

- **Customer-Specific Ratings**:
  - **Truthfulness**: Accuracy of the renter's description about the vehicle (pyhisical status, features, etc.)

- **Renter-Specific Ratings**:
  - **Vehicle Care**: How well the customer treated the vehicle during the rental

- **Overall User Rating**:  
  - Each user's total rating is automatically calculated as the average of all ratings received across completed rentals.



