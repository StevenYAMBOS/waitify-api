package config

import (
	"context"
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/aws/aws-sdk-go-v2/aws"
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

func ShowError(w http.ResponseWriter, r *http.Request, status int, message string) {
	http.Error(w, message, status)
}

func ListAWSS3Buckets(w http.ResponseWriter, r *http.Request) {

	// There aren't really any folders in S3, but we can emulate them by using "/" in the key names
	// of the objects. In case we want to listen the contents of a "folder" in S3, what we really need
	// to do is to list all objects which have a certain prefix.
	// prefix := r.URL.Query().Get("prefix")
	// delimeter := r.URL.Query().Get("delimeter")

	paginator := s3.NewListObjectsV2Paginator(awsS3Client, &s3.ListObjectsV2Input{
		Bucket: aws.String(os.Getenv("AWS_S3_BUCKET")),
		// Prefix:    aws.String(prefix),
		// Delimiter: aws.String(delimeter),
	})

	w.Header().Set("Content-Type", "text/html")

	for paginator.HasMorePages() {
		page, err := paginator.NextPage(context.TODO())
		if err != nil {
			// Error handling goes here
		}
		for _, obj := range page.Contents {
			// Do whatever you need with each object "obj"
			fmt.Fprintf(w, "<li>File %s</li>", *obj.Key)
		}
	}

	return
}

/*
type BucketBasics struct {
	S3Client *s3.Client
}

// Liste des buckets associés au compte
func (basics BucketBasics) ListBuckets(ctx context.Context) ([]types.Bucket, error) {
	var err error
	var output *s3.ListBucketsOutput
	var buckets []types.Bucket
	bucketPaginator := s3.NewListBucketsPaginator(basics.S3Client, &s3.ListBucketsInput{})
	for bucketPaginator.HasMorePages() {
		output, err = bucketPaginator.NextPage(ctx)
		if err != nil {
			var apiErr smithy.APIError
			if errors.As(err, &apiErr) && apiErr.ErrorCode() == "AccessDenied" {
				fmt.Println("You don't have permission to list buckets for this account.")
				err = apiErr
			} else {
				log.Printf("Couldn't list buckets for your account. Here's why: %v\n", err)
			}
			break
		} else {
			buckets = append(buckets, output.Buckets...)
		}
	}
	return buckets, err
}
*/
