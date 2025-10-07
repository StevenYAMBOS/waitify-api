package utils

import (
	"fmt"

	qrcode "github.com/skip2/go-qrcode"
)

// Modèle d'un QR Code
type QRCode struct {
	Content string
	Size    int
}

// Générer un QR Code
func (code *QRCode) Generate() ([]byte, error) {
	qrCode, err := qrcode.Encode(code.Content, qrcode.Medium, code.Size)
	if err != nil {
		return nil, fmt.Errorf("Erreur lors de la génération du QR Code : %v", err)
	}
	return qrCode, nil
}
