package com.stevenyambos.waitify_api;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.jdbc.DataSourceAutoConfiguration;

@SpringBootApplication(exclude = {DataSourceAutoConfiguration.class })
public class WaitifyApiApplication {

	public static void main(String[] args) {
		SpringApplication.run(WaitifyApiApplication.class, args);
	}

}
