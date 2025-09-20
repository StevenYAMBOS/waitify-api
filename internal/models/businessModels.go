package models

import (
	"errors"
	"log"
	"regexp"
	"time"

	"github.com/google/uuid"
)

type Business struct {
	ID                      uuid.UUID `json:"id" db:"id"`
	UserID                  uuid.UUID `json:"UserId" db:"UserId"`
	Name                    string    `json:"name" db:"name"`
	BusinessType            string    `json:"business_type" db:"business_type"`
	PhoneNumber             string    `json:"phone_number" db:"phone_number"`
	Address                 string    `json:"address" db:"address"`
	City                    string    `json:"city" db:"city"`
	ZipCode                 string    `json:"zip_code" db:"zip_code"`
	Country                 string    `json:"country" db:"country"`
	QRCodeToken             string    `json:"qr_code_token" db:"qr_code_token"`
	AverageServiceTime      int       `json:"average_service_time" db:"average_service_time"`
	IsQueueActive           bool      `json:"is_queue_active" db:"is_queue_active"`
	IsQueuePaused           bool      `json:"is_queue_paused" db:"is_queue_paused"`
	MaxQueueSize            int       `json:"max_queue_size" db:"max_queue_size"`
	OpeningHours            string    `json:"opening_hours" db:"opening_hours"`
	CustomMessage           string    `json:"custom_message" db:"custom_message"`
	SmsNotificationsEnabled bool      `json:"sms_notifications_enabled" db:"sms_notifications_enabled"`
	AutoAdvanceEnabled      bool      `json:"auto_advance_enabled" db:"auto_advance_enabled"`
	ClientTimeoutMinutes    int       `json:"client_timeout_minutes" db:"client_timeout_minutes"`
	IsActive                int       `json:"is_active" db:"is_active"`
	CreatedAt               time.Time `json:"created_at" db:"created_at"`
	UpdatedAt               time.Time `json:"updated_at" db:"updated_at"`
}

func (business *Business) ValidateBusinessType() error {
	var types []string = []string{"bakery", "hairdresser", "pharmacy", "garage", "restaurant",
		"medical_office", "dentist", "veterinary", "optician", "bank",
		"insurance", "notary", "lawyer", "accountant", "real_estate",
		"prefecture", "city_hall", "family_allowance", "employment_agency", "public_service",
		"post_office", "dry_cleaning", "cobbler", "watchmaker", "phone_repair",
		"beauty_salon", "massage", "tattoo", "nail_salon", "barber",
		"vehicle_inspection", "gas_station", "auto_body", "tire_service",
		"other"}

	for _, typeReceived := range types {
		if business.BusinessType != typeReceived {
			return errors.New("[user.go -> ValidatePhoneNumber()] -> Le numéro de téléphone contient trop ou pas assez de caractères.")
		}
	}
	return nil
}

// Format du numéro de téléphone
func (business *Business) ValidatePhoneNumber() error {
	if len(business.PhoneNumber) < 10 || len(business.PhoneNumber) > 13 {
		return errors.New("[user.go -> ValidatePhoneNumber()] -> Le numéro de téléphone contient trop ou pas assez de caractères.")
	}

	re := regexp.MustCompile(`^(?:(?:\(?(?:00|\+)([1-4]\d\d|[1-9]\d?)\)?)?[\-\.\ \\\/]?)?((?:\(?\d{1,}\)?[\-\.\ \\\/]?){0,})(?:[\-\.\ \\\/]?(?:#|ext\.?|extension|x)[\-\.\ \\\/]?(\d+))?$`)
	if !re.MatchString(business.PhoneNumber) {
		log.Println("[user.go] -> Phone number is not valid:", business.PhoneNumber)
		return errors.New("[user.go -> ValidatePhoneNumber()] -> Le numéro de téléphone n'est pas au format valide.")
	}
	return nil
}

// Format réponse auhtentification
type AddBusinessResponse struct {
	Response string   `json:"Response"`
	Business Business `json:"Business"`
}
