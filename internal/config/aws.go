package config

import (
	"context"
	"log"
	"os"

	"github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/s3"
)

var awsS3Client *s3.Client

func ConfigS3() {
	awsS3Config, err := config.LoadDefaultConfig(context.TODO(), config.WithRegion(os.Getenv("AWS_S3_REGION")))
	if err != nil {
		log.Fatal(err)
	}

	awsS3Client = s3.NewFromConfig(awsS3Config)
}
