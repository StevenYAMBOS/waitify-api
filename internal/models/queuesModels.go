package models

import (
	"time"

	"github.com/google/uuid"
)

type Queue struct {
	ID                uuid.UUID `json:"id" db:"id"`
	BusinessID        uuid.UUID `json:"BusinessId" db:"BusinessId"`
	Phone             string    `json:"phone" db:"phone"`
	ClientName        string    `json:"client_name" db:"client_name"`
	Position          int       `json:"position" db:"position"`
	EstimatedWaitTime int       `json:"estimated_wait_time" db:"estimated_wait_time"`
	Status            string    `json:"status" db:"status"`
	CalledAt          time.Time `json:"called_at" db:"called_at"`
	ServedAt          time.Time `json:"served_at" db:"served_at"`
	ActualServiceTime int       `json:"actual_service_time" db:"actual_service_time"`
	SmsSentCount      int       `json:"sms_sent_count" db:"sms_sent_count"`
	LastSmsSentAt     time.Time `json:"last_sms_sent_at" db:"last_sms_sent_at"`
	CreatedAt         time.Time `json:"created_at" db:"created_at"`
	UpdatedAt         time.Time `json:"updated_at" db:"updated_at"`
}

type JoinQueueRequest struct {
	BusinessID uuid.UUID `json:"business_id"`
	Phone      string    `json:"phone"`
	ClientName string    `json:"client_name"`
}

type JoinQueueResponse struct {
	Message string     `json:"message"`
	Entry   QueueEntry `json:"entry"`
}

type QueueEntry struct {
	ID                uuid.UUID `json:"id"`
	BusinessID        uuid.UUID `json:"business_id"`
	Phone             string    `json:"phone"`
	ClientName        string    `json:"client_name"`
	Position          int       `json:"position"`
	EstimatedWaitTime int       `json:"estimated_wait_time"` // en minutes
	Status            string    `json:"status"`
	CreatedAt         time.Time `json:"created_at"`
}
