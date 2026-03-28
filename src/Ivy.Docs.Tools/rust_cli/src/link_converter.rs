use regex::Regex;
use std::collections::HashSet;
use crate::utils;

pub struct LinkConverter {
    current_file_path: String,
    link_regex: Regex,
}

impl LinkConverter {
    pub fn new(current_file_path: &str) -> Self {
        Self {
            current_file_path: current_file_path.to_string(),
            link_regex: Regex::new(r"!?\[(.*?)\]\((.*?)\)").unwrap(),
        }
    }

    pub fn convert(&self, markdown: &str) -> (HashSet<String>, String) {
        let mut types = HashSet::new();
        
        let mut result = markdown.to_string();
        
        // Find all matches first to avoid recursive replacement issues
        let matches: Vec<(String, String, String)> = self.link_regex.captures_iter(markdown)
            .filter(|cap| !cap[0].starts_with("!"))
            .map(|cap| (cap[0].to_string(), cap[1].to_string(), cap[2].to_string()))
            .collect();
            
        for (full_match, text, link) in matches {
            if link.starts_with("app://") || link.starts_with("http://") || link.starts_with("https://") ||
               link.starts_with("mailto:") || link.starts_with("tel:") || link.starts_with("#") {
                continue;
            }

            let fragment_index = link.find('#');
            let (link_without_fragment, fragment) = match fragment_index {
                Some(i) => (&link[..i], &link[i..]),
                None => (link.as_str(), ""),
            };

            let path = utils::get_path_for_link(&self.current_file_path, link_without_fragment);
            let type_name = utils::get_type_name_from_path(&path);
            let app_id = utils::get_app_id_from_type_name(&type_name);

            types.insert(type_name.clone());
            let replacement = format!("[{}](app://{}{})", text, app_id, fragment);
            result = result.replace(&full_match, &replacement);
        }

        (types, result)
    }
}
