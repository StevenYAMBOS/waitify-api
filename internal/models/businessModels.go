package models

import (
	"errors"
	"log"
	"regexp"
)

type Business struct {
	PhoneNumber  string `json:"phone" db:"phone"`
	CompanyName  string `json:"company_name" db:"company_name"`
	Name         string `json:"name" db:"name"`
	BusinessType string `json:"business_type" db:"business_type"`
	Address      string `json:"address" db:"address"`
	City         string `json:"city" db:"city"`
	ZipCode      string `json:"zip_code" db:"zip_code"`
	Country      string `json:"country" db:"country"`
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
